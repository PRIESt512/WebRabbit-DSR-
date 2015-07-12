using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebCommandDevice.ControlCommand
{
    internal abstract class Command : IDisposable
    {
        public abstract String Execute(TimeSpan timeout);
        public abstract Int32 CountMessage();
        public abstract void UnExecute();

        public abstract void Dispose();
    }

    internal class DeviceCommand : Command
    {
        private readonly RabbitReceiner _rabbitReceiner;

        public DeviceCommand(RabbitReceiner rabbitReceiner)
        {
            this._rabbitReceiner = rabbitReceiner;
        }

        public override Int32 CountMessage()
        {
            return _rabbitReceiner.QueueCount();
        }

        public override String Execute(TimeSpan timeout)
        {
            return _rabbitReceiner.SelectCommand(timeout);
        }

        public override void UnExecute()
        {
            _rabbitReceiner.DeliveryCommand();
        }

        public override void Dispose()
        {
            _rabbitReceiner.Dispose();
        }
    }

    internal class RabbitReceiner : Rabbit
    {
        private ulong _deliveryTag;

        public RabbitReceiner(String deviceId)
        {
            _deviceId = deviceId;

            _factory = new ConnectionFactory() { HostName = _hostName };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchange, "direct");
            _queueName = _deviceId;
            _channel.QueueBind(_queueName, _exchange, _deviceId);

            _consumer = new QueueingBasicConsumer(_channel);
            _channel.BasicConsume(_queueName, false, _consumer);
        }

        public Int32 QueueCount()
        {
            return _consumer.Queue.Count();
        }

        public String SelectCommand(TimeSpan timeout)
        {
            JObject json = null;
            while (true)
            {
                BasicDeliverEventArgs ea;
                if (!_consumer.Queue.Dequeue((Int32)timeout.TotalMilliseconds, out ea)) return "NotFound";

                Byte[] body = null;
                body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                
                json = JObject.Parse(message);
                String commandName = json.SelectToken("$.commandName").Value<String>();

                if (!IsCommandExpire(commandName, ea.BasicProperties.Timestamp))
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    continue;
                }

                _deliveryTag = ea.DeliveryTag;
                break;
            }
            return json.ToString();
        }

        private static Boolean IsCommandExpire(String command, AmqpTimestamp unixTimestamp)
        {
            Int64 timeout;
            switch (command)
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

        private static Int64 NowUnixTime => (Int64)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

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
    
    public class Device : IDisposable
    {
        private readonly Command _command;

        public Device(String deviceId)
        {
            _command = new DeviceCommand(new RabbitReceiner(deviceId));
        }

        public bool CheckCommand => _command.CountMessage() > 0;

        public String GetCommand(TimeSpan timeout)
        {
            // if (CheckCommand) return String.Empty;
            // var e = _command.CountMessage();
            return _command.Execute(timeout);
        }

        public void CleanCommand()
        {
            _command?.UnExecute();
        }

        public void Dispose()
        {
            _command.Dispose();
        }
    }
}