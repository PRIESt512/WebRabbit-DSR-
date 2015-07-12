using System;
using System.Text;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;

namespace WebCommandDevice.ControlCommand
{

    public class RabbitSender : Rabbit
    {
        public RabbitSender(String deviceId)
        {
            _deviceId = deviceId;

            _factory = new ConnectionFactory() { HostName = _hostName };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchange, "direct");
            _queueName = deviceId;
            _channel.QueueDeclare(_queueName, false, false, false, null);
            _channel.QueueBind(_queueName, _exchange, _deviceId);
        }

        public void SendCommand(String command)
        {
            var body = Encoding.UTF8.GetBytes(command);

            var properties = _channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            
            AmqpTimestamp unixTime = new AmqpTimestamp((Int64)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
            properties.Timestamp = unixTime;
            _channel.BasicPublish(_exchange, _deviceId, properties, body);
        }

        protected override void Dispose(Boolean flag)
        {
            _connection.Close();
            _channel.Close();
            GC.SuppressFinalize(this);
        }
    }
}