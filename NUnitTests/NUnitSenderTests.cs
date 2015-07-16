using System;
using System.Text;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebCommandDevice.ControlCommand;

namespace NUnitTests
{
    [TestFixture]
    public class SenderTest : RabbitBase
    {
        #region
        public SenderTest()
        {
            _deviceId = "300";

            _factory = new ConnectionFactory() { HostName = _hostName };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchange, "direct");
            _queueName = _deviceId;
            _channel.QueueBind(_queueName, _exchange, _deviceId);

            _consumer = new QueueingBasicConsumer(_channel);
            _channel.BasicConsume(_queueName, true, _consumer);
        }

        protected override void Dispose(Boolean flag)
        {
            if (!flag) return;
            _connection.Close();
            _channel.Close();
            GC.SuppressFinalize(this);
        }

#endregion

        [Test]
        public void CommandSenderDevice300Returned()
        {
            using (var command = new RabbitSender(_deviceId))
                command.SendCommand("Это команда для девайса id=300!");

            BasicDeliverEventArgs ea;
            _consumer.Queue.Dequeue(300, out ea);

            var body = ea.Body;
            var response = Encoding.UTF8.GetString(body);

            Assert.AreEqual("Это команда для девайса id=300!", response);
        }

        [Test]
        public void ManyCommandSenderDevice300Returned()
        {
            using (var command = new RabbitSender(_deviceId))
                for (Int32 i = 1; i <= 100; i++)
                    command.SendCommand($"Это команда для id=300 номер {i}");

            for (Int32 i = 1; i <= 100; i++)
            {
                BasicDeliverEventArgs ea;
                _consumer.Queue.Dequeue(300, out ea);
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Assert.AreEqual($"Это команда для id=300 номер {i}", message);
            }
        }
    }

}
