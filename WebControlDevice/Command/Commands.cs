using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace WebControlDevice.Command
{
    public static class Commands
    {
        private static readonly String connect = "http://localhost:54863/api/commands";

        public async static Task SendCommand(String command)
        {
            var commandByte = Encoding.Default.GetBytes(command);

            using (var client = new HttpClient())
            {
                using (var memoryContent = new MemoryStream(commandByte))
                {
                    using (var content = new StreamContent(memoryContent))
                    {
                        var message = await client.PostAsync(connect, content);
                        message.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        public static String Delete(String id)
        {
            return new JObject(
                new JProperty("SendCommandDto",
                new JObject(
                    new JProperty("deviceId", id),
                    new JProperty("command",
                    new JObject(
                        new JProperty("commandName", "delete"),
                        new JProperty("parameters",
                        new JArray(
                            new JObject(
                                new JProperty("comma", "work")))))
               )))).ToString();
        }

        public static String GetInfo(String id)
        {
            return new JObject(
               new JProperty("SendCommandDto",
               new JObject(
                   new JProperty("deviceId", id),
                   new JProperty("command",
                   new JObject(
                       new JProperty("commandName", "getInfo"),
                       new JProperty("parameters",
                       new JArray(
                           new JObject(
                               new JProperty("comma", "work")))))
               )))).ToString();
        }

        public static String Upgrade(String id, String name, String value)
        {
            return new JObject(
                new JProperty("SendCommandDto",
                new JObject(
                    new JProperty("deviceId", id),
                    new JProperty("command",
                    new JObject(
                        new JProperty("commandName", "upgrade"),
                        new JProperty("parameters",
                        new JArray(
                            new JObject(
                                new JProperty("name", name),
                                new JProperty("value", value)))))
               )))).ToString();
        }

        public static String SetOnOff(String id, Boolean onOff)
        {
            return new JObject(
               new JProperty("SendCommandDto",
               new JObject(
                   new JProperty("deviceId", id),
                   new JProperty("command",
                   new JObject(
                       new JProperty("commandName", "setOnOff"),
                       new JProperty("parameters",
                       new JArray(
                           new JObject(
                               new JProperty("name", "switchOn"),
                               new JProperty("value", new JValue(onOff))))))
              )))).ToString();
        }
    }
}