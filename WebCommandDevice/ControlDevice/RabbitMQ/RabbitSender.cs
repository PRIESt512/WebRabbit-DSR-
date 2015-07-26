using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace WebCommandDevice.ControlDevice.RabbitMQ
{
    public sealed class RabbitSender : RabbitBase
    {
        public override void InstallationState(String deviceId)
        {
            if (_channel != null) return;

            _deviceId = deviceId;
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_exchange, "direct");
            _queueName = deviceId;

            _channel.QueueDeclare(_queueName, false, false, false, null);
            _channel.QueueBind(_queueName, _exchange, _deviceId);
        }

        public override void ResetState()
        {
            _channel.Close();
        }

        /// <summary>
        /// Отправка команды в очередь, где команда будет ожидать отправления на клиентское устройство
        /// </summary>
        /// <param name="command">Команда конкретному устройству</param>
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