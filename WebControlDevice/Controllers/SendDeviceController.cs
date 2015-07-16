using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebControlDevice.Command;

namespace WebControlDevice.Controllers
{
    public class SendDeviceController : ApiController
    {
        [HttpGet]
        [ActionName("GetInfo")]
        public async Task<IHttpActionResult> CommandGetInfo(String deviceId)
        {
            var command = Commands.GetInfo(deviceId);

            var log = new Logging();
            try
            {
                await log.SaveHistoryCommandAsync(deviceId, command);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            return await SendCommand(command);
        }

        [HttpGet]
        [ActionName("Upgrade")]
        public async Task<IHttpActionResult> CommandUpgrade(String deviceId, String name, String value)
        {
            var command = Commands.Upgrade(deviceId, name, value);

            var log = new Logging();
            try
            {
                await log.SaveHistoryCommandAsync(deviceId, command);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            return await SendCommand(command);
        }

        [HttpGet]
        [ActionName("setOnOff")]
        public async Task<IHttpActionResult> CommandSetOnOff(String deviceId, Boolean onOff)
        {
            var command = Commands.SetOnOff(deviceId, onOff);

            var log = new Logging();
            try
            {
                await log.SaveHistoryCommandAsync(deviceId, command);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            return await SendCommand(command);
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IHttpActionResult> CommandDelete(String deviceId)
        {
            var command = Commands.Delete(deviceId);

            var log = new Logging();
            try
            {
                await log.SaveHistoryCommandAsync(deviceId, command);
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
                response = await log.GetHistoryCommandAsync(deviceId);

            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return Ok(response);
        }

        /// <summary>
        /// Отправка команды веб-сервису, который взаимодействует с клиентскими устройствами
        /// </summary>
        /// <param name="command">Команда для отправления</param>
        /// <returns></returns>
        private async Task<IHttpActionResult> SendCommand(String command)
        {
            try
            {
                await SendCommand(command);
                return Ok("Команда отправлена успешно!");
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.InnerException != null ? $"{ex.InnerException.Message}. {ex.InnerException.InnerException.Message}" : $"{ex.Message}");
            }
        }
    }
}
