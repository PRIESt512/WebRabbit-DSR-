using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace WebCommandDevice.ControlCommand
{
    public interface IReceiveCommand : IDisposable
    {
        Boolean IsCommand();
        String GetCommand(TimeSpan timeSpan);
        Task<String> GetCommandAsync(TimeSpan timeSpan);
        Int32 CountCommand();
        void CleanCommand();
    }

    public interface ISenderCommand : IDisposable
    {
        void SenderCommand(String command);
        Task SenderCommandAsync(String command);
    }

    public abstract class RabbitBase : IDisposable
    {
        protected String _deviceId;
        protected static readonly String _exchange = "commands";

        protected static readonly ConnectionFactory _factory = new ConnectionFactory
        {
            UserName = ConfigurationManager.AppSettings.Get("username"),
            Password = ConfigurationManager.AppSettings.Get("password"),
            HostName = ConfigurationManager.AppSettings.Get("host"),
            VirtualHost = ConfigurationManager.AppSettings.Get("virtualhost")
        };
        protected String _queueName;
        protected QueueingBasicConsumer _consumer;
        protected IConnection _connection;
        protected IModel _channel;

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(Boolean flag);

        ~RabbitBase()
        {
            Dispose(false);
        }

    }
}
