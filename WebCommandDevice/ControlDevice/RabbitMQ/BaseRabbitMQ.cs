using System;
using System.Configuration;
using System.Threading.Tasks;
using RabbitMQ.Client;
using WebCommandDevice.ControlDevice.Comet;
using WebCommandDevice.ControlDevice.Pool;

namespace WebCommandDevice.ControlDevice.RabbitMQ
{
    public interface IReceiveCommand : IDisposable
    {
        Boolean IsCommand();
        void GetCommand(TimeSpan? timeSpan, out Command commandBody);   
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
            UserName = ConfigurationManager.AppSettings.Get("username"),
            Password = ConfigurationManager.AppSettings.Get("password"),
            HostName = ConfigurationManager.AppSettings.Get("host"),
            VirtualHost = ConfigurationManager.AppSettings.Get("virtualhost"),
            Port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("port"))
        };
       
        protected static IConnection _connection = _factory.CreateConnection();
        protected IModel _channel;

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
