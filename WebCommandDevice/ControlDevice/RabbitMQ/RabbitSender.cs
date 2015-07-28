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
            this._queueName = this._deviceId = deviceId;
            this._channel = _connection.CreateModel();
            this._channel.ExchangeDeclare(_exchange, "direct");
            this._channel.QueueDeclare(this._queueName, false, false, false, null);
            this._channel.QueueBind(this._queueName, _exchange, this._deviceId);
        }

        public override void ResetState()
        {
            //this._channel.QueueUnbind(this._queueName, _exchange, this._deviceId, null);
            this._channel.Close();
        }

        /// <summary>
        /// Отправка команды в очередь, где команда будет ожидать отправления на клиентское устройство
        /// </summary>
        /// <param name="command">Команда конкретному устройству</param>
        public void SendCommand(String command)
        {
            var body = Encoding.UTF8.GetBytes(command);

            var properties = this._channel.CreateBasicProperties();
            properties.DeliveryMode = 2;

            AmqpTimestamp unixTime = new AmqpTimestamp((Int64)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
            properties.Timestamp = unixTime;
            this._channel.BasicPublish(_exchange, this._deviceId, properties, body);
        }

        public Task SendCommandAsync(String command)
        {
            return Task.Run(() => this.SendCommand(command));
        }
        protected override void Dispose(Boolean flag)
        {
            if (!flag) return;
            this._channel.Close();
            GC.SuppressFinalize(this);
        }
    }

    public partial class Device<T>
    {
        public void SenderCommand(String command)
        {
            this._sender.SendCommand(command);
        }

        public Task SenderCommandAsync(String command)
        {
            return this._sender.SendCommandAsync(command);
        }
    }
}