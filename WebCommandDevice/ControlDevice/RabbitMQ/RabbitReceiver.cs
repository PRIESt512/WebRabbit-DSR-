using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebCommandDevice.ControlDevice.RabbitMQ
{
    public sealed class RabbitReceiner : RabbitBase
    {
        BasicDeliverEventArgs _ea;

        public override void InstallationState(String deviceId)
        {
            _deviceId = deviceId;

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchange, "direct");
            _queueName = _deviceId;
            _channel.QueueBind(_queueName, _exchange, _deviceId);

            _consumer = new QueueingBasicConsumer(_channel);

            _channel.BasicConsume(_queueName, false, _consumer);
        }

        public override void ResetState()
        {
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
        public String GetCommand(TimeSpan? timeSpan)
        {
            String message;
            Byte[] body;
            AmqpTimestamp timestamp;

            while (true)
            {
                if (!_receiner.SelectCommand(out body, out timestamp)) return "NotFound";
                message = Encoding.UTF8.GetString(body);

                var json = JObject.Parse(message);
                _commandName.Append(json.SelectToken("$.commandName").Value<String>());

                if (!IsCommandExpire(_commandName, timestamp))
                {
                    _receiner.DeliveryCommand();
                    _commandName.Clear();
                    continue;
                }
                _commandName.Clear();
                break;
            }
            return message;
        }

        private Boolean IsCommandExpire(StringBuilder command, AmqpTimestamp unixTimestamp)
        {
            Int64 timeout;
            switch (command.ToString())
            {
                case "delete":
                    timeout = (Int64)CommandTime.Delete;
                    break;
                case "getInfo":
                    timeout = (Int64)CommandTime.GetInfo;
                    break;
                case "upgrade":
                    timeout = (Int64)CommandTime.Upgrade;
                    break;
                case "setOnOff":
                    timeout = (Int64)CommandTime.SetOnOff;
                    break;
                default:
                    timeout = 10000;
                    break;
            }
            return (NowUnixTime - unixTimestamp.UnixTime) <= timeout;
        }

        private Int64 NowUnixTime
        {
            get { return (Int64)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds; }
        }

        /// <summary>
        /// Асинхронное получение команды из очереди, в случае отсутствия режим ожидания
        /// не приводит к блокированию вызывающего потока
        /// </summary>
        /// <param name="timeSpan">Наличие параметра приводит к временному ожидания команды, 
        /// в противном случае бесконечное ожидание</param>
        /// <returns>Текст команды</returns>
        public Task<String> GetCommandAsync(TimeSpan? timeSpan)
        {
            return new TaskFactory().StartNew(() => GetCommand(timeSpan),
                TaskCreationOptions.LongRunning);
        }

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
            GetInfo = 30,
            Upgrade = 30,
            SetOnOff = 20
        }
    }
}