using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebCommandDevice.ControlCommand;

namespace NUnitTests
{
    [TestFixture]
    class NUnitReceiveTests : RabbitBase
    {
        #region
        public NUnitReceiveTests()
        {
            _deviceId = "300";
        }

        protected override void Dispose(bool flag)
        {
            if (!flag) return;
            _connection.Close();
            _channel.Close();
            GC.SuppressFinalize(this);
        }
        #endregion
        /// <summary>
        /// Данные для проверки были подготовлены заранне: отправлены с веб-сервиса
        /// </summary>
        [Test]
        public void CommandReceiveGetInfoDevice300Returned()
        {
            using (var receive = new RabbitReceiner("300"))
            {
                var response = receive.SelectCommand();

                var json = new JObject(
                    new JProperty("commandName", "getInfo"),
                    new JProperty("parameters",
                    new JArray(
                        new JObject(
                            new JProperty("comma", "work")))));

                Assert.AreEqual(json.ToString(), response);
                receive.DeliveryCommand();
            }
        }

        [Test]
        public void CommandReceiveGetInfoDevice300TimeoutReturned()
        {
            using (var receive = new RabbitReceiner("300"))
            {
                var response = receive.SelectCommand(new TimeSpan(0, 0, 2));

                Assert.AreEqual("NotFound", response);
                receive.DeliveryCommand();
            }
        }
    }
}
