using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebControlDevice.Command
{
    public static class ControlDevice
    {
        #region Классы для Сериализации
        private class Command
        {
            public String commandName;

            public Dictionary<String, Object> parameters;
        }

        private class Device
        {
            public String deviceId;

            public Command command;
        }

        private class SendDto
        {
            public Device SendCommandDto;

            public String GetJsonSendCommand()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
        #endregion

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

        public static String GetInfo(String id)
        {
            var command = new Command
            {
                commandName = "getInfo",
                parameters = new Dictionary<String, Object>
                {
                    { "comma", "work"}
                }
            };

            var device = new Device
            {
                deviceId = id,
                command = command
            };

            var sendCommand = new SendDto
            {
                SendCommandDto = device
            };

            return sendCommand.GetJsonSendCommand();

            //return new JObject(
            //   new JProperty("SendCommandDto",
            //   new JObject(
            //       new JProperty("deviceId", id),
            //       new JProperty("command",
            //       new JObject(
            //           new JProperty("commandName", "getInfo"),
            //           new JProperty("parameters",
            //           new JArray(
            //               new JObject(
            //                   new JProperty("comma", "work")))))
            //   )))).ToString();
        }

        public static String Delete(String id)
        {
            var command = new Command
            {
                commandName = "delete",
                parameters = new Dictionary<String, Object>
                {
                    { "comma", "work"}
                }
            };
            var device = new Device
            {
                deviceId = id,
                command = command
            };

            var sendCommand = new SendDto
            {
                SendCommandDto = device
            };

            return sendCommand.GetJsonSendCommand();

            //return new JObject(
            //    new JProperty("SendCommandDto",
            //    new JObject(
            //        new JProperty("deviceId", id),
            //        new JProperty("command",
            //        new JObject(
            //            new JProperty("commandName", "delete"),
            //            new JProperty("parameters",
            //            new JArray(
            //                new JObject(
            //                    new JProperty("comma", "work")))))
            //   )))).ToString();
        }



        public static String Upgrade(String id, String name, String value)
        {
            var command = new Command
            {
                commandName = "upgrade",
                parameters = new Dictionary<String, Object>
                {
                    { "name", name},
                    {value, value }
                }
            };
            var device = new Device
            {
                deviceId = id,
                command = command
            };

            var sendCommand = new SendDto
            {
                SendCommandDto = device
            };

            return sendCommand.GetJsonSendCommand();

            //return new JObject(
            //    new JProperty("SendCommandDto",
            //    new JObject(
            //        new JProperty("deviceId", id),
            //        new JProperty("command",
            //        new JObject(
            //            new JProperty("commandName", "upgrade"),
            //            new JProperty("parameters",
            //            new JArray(
            //                new JObject(
            //                    new JProperty("name", name),
            //                    new JProperty("value", value)))))
            //   )))).ToString();
        }

        public static String SetOnOff(String id, Boolean onOff)
        {
            var command = new Command
            {
                commandName = " setOnOff",
                parameters = new Dictionary<String, Object>
                {
                    { "name", "switchOn"},
                    { "value", onOff }
                }
            };

            var device = new Device
            {
                deviceId = id,
                command = command
            };

            var sendCommand = new SendDto
            {
                SendCommandDto = device
            };

            return sendCommand.GetJsonSendCommand();

            //return new JObject(
            //   new JProperty("SendCommandDto",
            //   new JObject(
            //       new JProperty("deviceId", id),
            //       new JProperty("command",
            //       new JObject(
            //           new JProperty("commandName", "setOnOff"),
            //           new JProperty("parameters",
            //           new JArray(
            //               new JObject(
            //                   new JProperty("name", "switchOn"),
            //                   new JProperty("value", new JValue(onOff))))))
            //  )))).ToString();
        }
    }
}