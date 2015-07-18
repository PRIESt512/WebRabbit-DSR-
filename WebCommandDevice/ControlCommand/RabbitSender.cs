using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Util;

namespace WebCommandDevice.ControlCommand
{
    public class RabbitSender : RabbitBase
    {
        public RabbitSender(String deviceId)
        {
            _deviceId = deviceId;
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchange, "direct");
            _queueName = deviceId;

            _channel.QueueDeclare(_queueName, false, false, false, null);
            _channel.QueueBind(_queueName, _exchange, _deviceId);
        }

        /// <summary>
        /// �������� ������� � �������, ��� ������� ����� ������� ����������� �� ���������� ����������
        /// </summary>
        /// <param name="command">������� ����������� ����������</param>
        public void SendCommand(String command)
        {
            var body = Encoding.UTF8.GetBytes(command);

            var properties = _channel.CreateBasicProperties();
            properties.DeliveryMode = 2;

            AmqpTimestamp unixTime = new AmqpTimestamp((Int64)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
            properties.Timestamp = unixTime;
            _channel.BasicPublish(_exchange, _deviceId, properties, body);
        }

        public Task SendCommandAsync(String command)
        {
            return Task.Run(() => SendCommand(command));
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
        public void SenderCommand(String command)
        {
            _sender.SendCommand(command);
        }

        public Task SenderCommandAsync(String command)
        {
            return _sender.SendCommandAsync(command);
        }
    }
}