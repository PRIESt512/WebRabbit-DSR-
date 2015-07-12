using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace WebCommandDevice.ControlCommand
{
    public abstract class Rabbit : IDisposable
    {
        protected String _deviceId;
        protected static readonly String _exchange = "commads";
        protected static readonly String _hostName = "localhost";
        protected String _queueName;
        protected ConnectionFactory _factory;
        protected QueueingBasicConsumer _consumer;
        protected IConnection _connection;
        protected IModel _channel;

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(Boolean flag);

        ~Rabbit()
        {
            Dispose(false);
        }

    }
}
