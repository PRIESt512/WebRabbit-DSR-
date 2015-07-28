using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebCommandDevice.ControlDevice.Comet;

namespace WebCommandDevice.ControlDevice.RabbitMQ
{
    [Serializable()]
    public class NotFoundQueue : Exception
    {
        public NotFoundQueue() : base() { }
        public NotFoundQueue(String message) : base(message) { }
        public NotFoundQueue(String message, System.Exception inner) : base(message, inner) { }

        protected NotFoundQueue(System.Runtime.Serialization.SerializationInfo info,
       System.Runtime.Serialization.StreamingContext context)
        { }
    }

    public sealed class RabbitReceiner : RabbitBase
    {
        private BasicDeliverEventArgs _ea;
        private QueueingBasicConsumer _consumer;

        public override void InstallationState(String deviceId)
        {
            try
            {
                #region не удачный вариант
                //if (this._channel != null)
                //{
                //    this._channel.QueueBind(_queueName, _exchange, _deviceId);
                //    this._consumer = new QueueingBasicConsumer(_channel);
                //    this._channel.BasicConsume(_queueName, false, _consumer);
                //}
                //else
                //{
                //    this._queueName = _deviceId = deviceId;
                //    this._channel = _connection.CreateModel();
                //    this._channel.ExchangeDeclare(_exchange, "direct");

                //    this._channel.QueueBind(_queueName, _exchange, _deviceId);
                //    this._consumer = new QueueingBasicConsumer(_channel);
                //    this._channel.BasicConsume(_queueName, false, _consumer);
                //}
                    #endregion
                this._queueName = _deviceId = deviceId;
                this._channel = _connection.CreateModel();
                this._channel.ExchangeDeclare(_exchange, "direct");

                this._channel.QueueBind(_queueName, _exchange, _deviceId);
                this._consumer = new QueueingBasicConsumer(_channel);
                this._channel.BasicConsume(_queueName, false, _consumer);
            }
            catch (Exception ex)
            {
                throw new NotFoundQueue(String.Format("Queue {0} Not Found", _queueName));
            }
        }

        public override void ResetState()
        {
            //_channel.BasicCancel(_consumer.ConsumerTag);
            //_consumer.OnCancel();
            //_channel.QueueUnbind(_queueName, _exchange, _deviceId, null);
            _channel.Close();
        }

        public Int32 CountCommands()
        {
            return _consumer.Queue.Count();
        }

        /// <summary>
        /// Проверка на наличие команд 
        /// </summary>
        /// <param name="timeout">Необязательный параметр, с ним ожидание команды в течение определенного промежутка времени; 
        /// без него бесконечное ожидание команды </param>
        /// <returns>null, если очередь пуста</returns>
        public Boolean SelectCommand(out Byte[] bodyBytes, out AmqpTimestamp timestamp, TimeSpan? timeout = null)
        {
            if (timeout == null) _ea = _consumer.Queue.Dequeue();
            else if (!_consumer.Queue.Dequeue((Int32)timeout.Value.TotalMilliseconds, out _ea))
            {
                bodyBytes = null;
                timestamp = default(AmqpTimestamp);
                return false;
            }

            bodyBytes = _ea.Body;
            timestamp = _ea.BasicProperties.Timestamp;
            return true;
        }

        /// <summary>
        /// Удаление команды из очереди
        /// </summary>
        public void DeliveryCommand()
        {
            _channel.BasicAck(_ea.DeliveryTag, false);
        }

        protected override void Dispose(Boolean flag)
        {
            if (!flag) return;
            _channel.Close();
            GC.SuppressFinalize(this);
        }
    }

    public partial class Device<T>
    {
        private static StringBuilder _commandName = new StringBuilder();

        public void CleanCommand()
        {
            _receiner.DeliveryCommand();
        }

        public Int32 CountCommand()
        {
            return _receiner.CountCommands();
        }
        
        /// <summary>
        /// Получение команды из очереди, в случае отсутствия режим ожидания, 
        /// приводит к блокированию вызывающего потока
        /// </summary>
        /// <param name="timeSpan">Наличие параметра приводит к временному ожидания команды, 
        /// в противном случае бесконечное ожидание</param>
        /// <returns>Текст команды</returns>
        public void GetCommand(TimeSpan? timeSpan, out Command command)
        {
            Byte[] body;
            AmqpTimestamp timestamp;
            String message;
            while (true)
            {
                if (!_receiner.SelectCommand(out body, out timestamp, timeSpan))
                {
                    command = new Command(false);
                    return;
                }
                message = Encoding.UTF8.GetString(body);

                var json = JObject.Parse(message);
                _commandName.Append(json.SelectToken("$.commandName").Value<String>());
               
                if (!IsCommandExpire(_commandName, timestamp))
                {
                    _receiner.DeliveryCommand();
                    _commandName.Clear();
                    timestamp = new AmqpTimestamp(timestamp.UnixTime / 2);
                    continue;
                }
                _commandName.Clear();
                break;
            }
            command = new Command(message);
        }

        private Boolean IsCommandExpire(StringBuilder command, AmqpTimestamp unixTimestamp)
        {
            Int64 timeout;
            switch (command.ToString().ToLower())
            {
                case "delete":
                    timeout = (Int64)CommandTime.Delete;
                    break;
                case "getinfo":
                    timeout = (Int64)CommandTime.GetInfo;
                    break;
                case "upgrade":
                    timeout = (Int64)CommandTime.Upgrade;
                    break;
                case "setonoff":
                    timeout = (Int64)CommandTime.SetOnOff;
                    break;
                default:
                    timeout = 1000;
                    break;
            }
            return (NowUnixTime - unixTimestamp.UnixTime) <= timeout;
        }

        private Int64 NowUnixTime
        {
            get { return ((Int64)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds); }
        }

        /// <summary>
        /// Асинхронное получение команды из очереди, в случае отсутствия режим ожидания
        /// не приводит к блокированию вызывающего потока
        /// </summary>
        /// <param name="timeSpan">Наличие параметра приводит к временному ожидания команды, 
        /// в противном случае бесконечное ожидание</param>
        /// <returns>Текст команды</returns>
        //public Task<String> GetCommandAsync(TimeSpan? timeSpan)
        //{
        //    return new TaskFactory().StartNew(() => GetCommand(timeSpan),
        //        TaskCreationOptions.LongRunning);
        //}

        public Boolean IsCommand()
        {
            return _receiner.CountCommands() > 0;
        }

        /// <summary>
        /// Таймаут конкретной команды
        /// </summary>
        public enum CommandTime
        {
            Delete = 15,
            GetInfo = 5,
            Upgrade = 30,
            SetOnOff = 10
        }
    }
}