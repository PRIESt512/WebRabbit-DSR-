using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using WebCommandDevice.ControlDevice.Pool;

namespace WebCommandDevice.ControlDevice.RabbitMQ
{
    public interface IReceiveCommand : IDisposable
    {
        Boolean IsCommand();
        String GetCommand(TimeSpan? timeSpan);
        Task<String> GetCommandAsync(TimeSpan? timeSpan);
        Int32 CountCommand();
        void CleanCommand();
    }

    public interface ISenderCommand : IDisposable
    {
        void SenderCommand(String command);
        Task SenderCommandAsync(String command);
    }

    public abstract class RabbitBase : IDisposable, IPoolable
    {
        protected static readonly ConnectionFactory _factory = new ConnectionFactory
        {
            UserName = "admin",
            Password = "mistral",
            HostName = "localhost",
            VirtualHost = "device"
        };

        protected static IConnection _connection = _factory.CreateConnection();
        protected IModel _channel;
        protected QueueingBasicConsumer _consumer;

        protected static readonly String _exchange = "commands";
        protected String _deviceId;
        protected String _queueName;

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(Boolean flag);

        ~RabbitBase()
        {
            Dispose(false);
        }

        public abstract void InstallationState(String deviceId);
        public abstract void ResetState();
    }
}
