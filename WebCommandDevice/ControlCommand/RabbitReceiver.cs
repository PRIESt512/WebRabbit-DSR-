using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebCommandDevice.ControlCommand
{
    public class RabbitReceiner : RabbitBase
    {
        private ulong _deliveryTag;
        private static StringBuilder _commandName = new StringBuilder();

        public RabbitReceiner(String deviceId)
        {
            _deviceId = deviceId;
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchange, "direct");
            _queueName = _deviceId;
            _channel.QueueBind(_queueName, _exchange, _deviceId);

            _consumer = new QueueingBasicConsumer(_channel);
            _channel.BasicConsume(_queueName, false, _consumer);
        }

        public Int32 CountCommands()
        {
            return _consumer.Queue.Count();
        }

        /// <summary>
        /// Проверка на наличие команд и выбор из очереди
        /// </summary>
        /// <param name="timeout">Необязательный параметр, без него доступ к очереди без ожидания</param>
        /// <returns>"NotFound" если очередь пуста</returns>
        public String SelectCommand(TimeSpan? timeout = null)
        {
            JObject json = null;
            while (true)
            {
                BasicDeliverEventArgs ea = null;
                Byte[] body = null;

                if (timeout == null) ea = _consumer.Queue.Dequeue();
                else if (!_consumer.Queue.Dequeue((Int32)timeout.Value.TotalMilliseconds, out ea)) return "NotFound";

                body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                json = JObject.Parse(message);
                _commandName.Append(json.SelectToken("$.commandName").Value<String>());

                if (!IsCommandExpire(_commandName, ea.BasicProperties.Timestamp))
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _commandName.Clear();
                    continue;
                }
                _deliveryTag = ea.DeliveryTag;
                _commandName.Clear();
                break;
            }
            return json.ToString();
        }

        /// <summary>
        /// Асинхронный выбор команд
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<String> SelectCommandAsync(TimeSpan? timeout = null)
        {
            return new TaskFactory<String>().StartNew(() => SelectCommand(timeout),
                TaskCreationOptions.LongRunning);
        }

        private static Boolean IsCommandExpire(StringBuilder command, AmqpTimestamp unixTimestamp)
        {
            Int64 timeout;
            switch (command.ToString())
            {
                case "delete":
                    timeout = (Int64)DeviceManager.CommandTime.Delete;
                    break;
                case "getInfo":
                    timeout = (Int64)DeviceManager.CommandTime.GetInfo;
                    break;
                case "upgrade":
                    timeout = (Int64)DeviceManager.CommandTime.Upgrade;
                    break;
                case "setOnOff":
                    timeout = (Int64)DeviceManager.CommandTime.SetOnOff;
                    break;
                default:
                    timeout = 10000;
                    break;
            }
            return (NowUnixTime - unixTimestamp.UnixTime) <= timeout;
        }

        private static Int64 NowUnixTime
        {
            get { return (Int64) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds; }
        }

        /// <summary>
        /// Удаление команды из очереди
        /// </summary>
        public void DeliveryCommand()
        {
            _channel.BasicAck(_deliveryTag, false);
        }

        protected override void Dispose(Boolean flag)
        {
            if (!flag) return;
            _connection.Close();
            _channel.Close();
            GC.SuppressFinalize(this);
        }
    }

    public partial class Device<T>
    {
        public void CleanCommand()
        {
            _receiner.DeliveryCommand();
        }

        public Int32 CountCommand()
        {
            return _receiner.CountCommands();
        }

        public string GetCommand(TimeSpan timeSpan)
        {
            return _receiner.SelectCommand(timeSpan);
        }

        public Task<String> GetCommandAsync(TimeSpan timeSpan)
        {
            return _receiner.SelectCommandAsync(timeSpan);
        }

        public Boolean IsCommand()
        {
            return _receiner.CountCommands() > 0;
        }
    }
}