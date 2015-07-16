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
        [ActionName("GetInfo")]
        public async Task<IHttpActionResult> CommandGetInfo(String deviceId)
        {
            var command = Commands.Commands.GetInfo(deviceId);

            var log = new Logging();
            try
            {
                await log.SaveCommandDeviceAsync(deviceId, command.ToString());
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            return await SendCommand(command.ToString());
        }

        [HttpGet]
        [ActionName("Upgrade")]
        public async Task<IHttpActionResult> CommandUpgrade(String deviceId, String name, String value)
        {
            var command = Commands.Commands.Upgrade(deviceId, name, value);

            var log = new Logging();
            try
            {
                await log.SaveCommandDeviceAsync(deviceId, command.ToString());
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            return await SendCommand(command.ToString());
        }

        [HttpGet]
        [ActionName("setOnOff")]
        public async Task<IHttpActionResult> CommandSetOnOff(String deviceId, Boolean onOff)
        {
            var command = Commands.Commands.SetOnOff(deviceId, onOff);

            var log = new Logging();
            try
            {
                await log.SaveCommandDeviceAsync(deviceId, command);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            return await SendCommand(command);
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IHttpActionResult> GetCommandDelete(String deviceId)
        {
            var command = Commands.Commands.Delete(deviceId);

            var log = new Logging();
            try
            {
                await log.SaveCommandDeviceAsync(deviceId, command);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            return await SendCommand(command);
        }

        [HttpGet]
        [ActionName("history")]
        public async Task<IHttpActionResult> HistoryDevice(String deviceId)
        {
            var log = new Logging();
            List<String> response = null;
            try
            {
                response = await log.CommandHistory(deviceId);

            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok(response);
        }

        private async Task<IHttpActionResult> SendCommand(String command)
        {
            try
            {
                await Device.SendCommand(command);
                return Ok("Команда отправлена успешно!");
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.InnerException != null ? $"{ex.InnerException.Message}. {ex.InnerException.InnerException.Message}" : $"{ex.Message}");
            }
        }
    }
}
