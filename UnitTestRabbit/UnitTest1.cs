using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTestRabbit
{
    [TestClass]
    public class UnitTest1
    {
        private static HttpClient _client;
        private static readonly List<String> _deviceId = new List<String> { "dev1", "dev2", "dev3" };
        private static readonly List<String> _timeout = new List<String> { "4", "6", "1000" };
        private static readonly List<String> _commandName = new List<String> { "setOnOff", "upgrade" };

        /// <summary>
        /// Проверка на наличие отправленных команд
        /// </summary>
        [TestMethod]
        public async void TestMethod1()
        {
            using (_client = new HttpClient())
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);

                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartObject();
                    writer.WritePropertyName("deviceId");
                    writer.WriteValue(_deviceId[0]);
                    writer.WritePropertyName("command");
                    writer.WriteStartObject();
                    writer.WritePropertyName("commandName");
                    writer.WriteValue(_commandName[0]);
                    writer.WritePropertyName("parameters");
                    writer.WriteStartArray();
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteValue("switchOn");
                    writer.WritePropertyName("value");
                    writer.WriteValue("true");
                    writer.WriteEndObject();
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    writer.WriteEndObject();

                    var content = new StringContent(sb.ToString());
                    await _client.PostAsync("http://localhost:54863/commands", content);
                }

                var response = await _client.GetAsync("http://localhost:54863/commands/" + _deviceId[0] + "/" + _timeout[0]);

                var expected = JObject.Parse(await response.Content.ReadAsStringAsync());
                var actual = JObject.Parse(sb.ToString());

                Assert.AreEqual(expected.SelectToken("$..command"), actual.SelectToken("$..command"));
            }
        }

        /// <summary>
        /// Проверка нескольких команд
        /// </summary>
        [TestMethod]
        public async void TestMethod2()
        {
            for (int i = 0; i < 2; i++)
            {
                using (_client = new HttpClient())
                {
                    var sb = new StringBuilder();
                    var sw = new StringWriter(sb);

                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;

                        writer.WriteStartObject();
                        writer.WritePropertyName("deviceId");
                        writer.WriteValue(_deviceId[i]);
                        writer.WritePropertyName("command");
                        writer.WriteStartObject();
                        writer.WritePropertyName("commandName");
                        writer.WriteValue(_commandName[i]);
                        writer.WritePropertyName("parameters");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        writer.WritePropertyName("name");
                        writer.WriteValue("switchOn");
                        writer.WritePropertyName("value");
                        writer.WriteValue("true");
                        writer.WriteEndObject();
                        writer.WriteEndArray();
                        writer.WriteEndObject();
                        writer.WriteEndObject();

                        var content = new StringContent(sb.ToString());
                        await _client.PostAsync("http://localhost:54863/commands", content);
                    }

                    var response = await _client.GetAsync("http://localhost:54863/commands/" + _deviceId[i] + "/" + _timeout[i]);

                    var expected = JObject.Parse(await response.Content.ReadAsStringAsync());
                    var actual = JObject.Parse(sb.ToString());

                    Assert.AreEqual(expected.SelectToken("$..command").Value<String>(), actual.SelectToken("$..command").Value<String>());
                }
            }
        }

        /// <summary>
        /// Проверка на таймаут
        /// </summary>
        [TestMethod]
        public async void TestMethod3()
        {
            using (_client = new HttpClient())
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);

                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartObject();
                    writer.WritePropertyName("deviceId");
                    writer.WriteValue(_deviceId[0]);
                    writer.WritePropertyName("command");
                    writer.WriteStartObject();
                    writer.WritePropertyName("commandName");
                    writer.WriteValue(_commandName[0]);
                    writer.WritePropertyName("parameters");
                    writer.WriteStartArray();
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteValue("switchOn");
                    writer.WritePropertyName("value");
                    writer.WriteValue("true");
                    writer.WriteEndObject();
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    writer.WriteEndObject();

                    var content = new StringContent(sb.ToString());
                    await _client.PostAsync("http://localhost:54863/commands", content);
                }

                var time = new TimeSpan(0, 0, Convert.ToInt32(_timeout[1]));
                Thread.Sleep(time);

                var response = await _client.GetAsync("http://localhost:54863/commands/" + _deviceId[0] + "/" + _timeout[1]);


                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
