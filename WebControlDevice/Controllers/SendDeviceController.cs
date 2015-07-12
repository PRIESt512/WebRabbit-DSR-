using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using WebControlDevice.Commands;

namespace WebControlDevice.Controllers
{
    public class SendDeviceController : ApiController
    {
        [HttpGet]
        [ActionName("Delete")]
        public async Task<IHttpActionResult> GetCommandDelete(String deviceId)
        {
            var command = new JObject(
                new JProperty("SendCommandDto",
                new JObject(
                    new JProperty("deviceId", deviceId),
                    new JProperty("command",
                    new JObject(
                        new JProperty("commandName", "delete"),
                        new JProperty("parameters",
                        new JArray(
                            new JObject(
                                new JProperty("comma", "work")))))
               ))));

            var log = new Logging();
            await log.SaveCommandDeviceAsync(deviceId, command.ToString());
            return await SendCommand(command.ToString());
        }

        [HttpGet]
        [ActionName("GetInfo")]
        public async Task<IHttpActionResult> CommandGetInfo(String deviceId)
        {

            var command = new JObject(
                new JProperty("SendCommandDto",
                new JObject(
                    new JProperty("deviceId", deviceId),
                    new JProperty("command",
                    new JObject(
                        new JProperty("commandName", "getInfo"),
                        new JProperty("parameters",
                        new JArray(
                            new JObject(
                                new JProperty("comma", "work")))))
                ))));

            return await SendCommand(command.ToString());
        }

        [HttpGet]
        [ActionName("Upgrade")]
        public async Task<IHttpActionResult> CommandUpgrade(String deviceId)
        {
            var command = new JObject(
                new JProperty("SendCommandDto",
                new JObject(
                    new JProperty("deviceId", deviceId),
                    new JProperty("command",
                    new JObject(
                        new JProperty("commandName", "upgrade"),
                        new JProperty("parameters",
                        new JArray(
                            new JObject(
                                new JProperty("name", "url"),
                                new JProperty("value", "goto")))))
               ))));

            return await SendCommand(command.ToString());
        }

        [HttpGet]
        [ActionName("setOnOff")]
        public async Task<IHttpActionResult> CommandSetOnOff(String deviceId, Boolean onOff)
        {
            var command = new JObject(
               new JProperty("SendCommandDto",
               new JObject(
                   new JProperty("deviceId", deviceId),
                   new JProperty("command",
                   new JObject(
                       new JProperty("commandName", "setOnOff"),
                       new JProperty("parameters",
                       new JArray(
                           new JObject(
                               new JProperty("name", "url"),
                               new JProperty("value", new JValue(onOff))))))
              ))));
            return await SendCommand(command.ToString());
        }

        [HttpGet]
        [ActionName("history")]
        public async Task<IHttpActionResult> HistoryDevice(String deviceId)
        {
            var log = new Logging();
            var response = await log.CommandHistory(deviceId);

            return Ok(response);
        }

        private async Task<IHttpActionResult> SendCommand(String command)
        {
            try
            {
                await Device.SendCommand(command);
                return Ok();
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.InnerException != null ? $"{ex.InnerException.Message}. {ex.InnerException.InnerException.Message}" : $"{ex.Message}");
            }
        }
    }
}
