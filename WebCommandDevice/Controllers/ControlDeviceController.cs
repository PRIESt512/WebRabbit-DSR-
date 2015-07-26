using System;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using WebCommandDevice.ControlDevice;
using WebCommandDevice.ControlDevice.Pool;
using WebCommandDevice.ControlDevice.RabbitMQ;

namespace WebCommandDevice.Controllers
{
    public class ControlDeviceController : ApiController
    {
        [HttpGet]
        [ActionName("commands")]
        public async Task<IHttpActionResult> CheckCommand(String deviceId, Int32 timeout)
        {
            var _timeout = new TimeSpan(0, 0, 0, timeout);
            String result = String.Empty;
            JObject response = new JObject();
            using (IReceiveCommand command = new Device<IReceiveCommand>(deviceId))
            {
                result = await command.GetCommandAsync(_timeout);

                if (result.Equals("NotFound")) return NotFound();

                if (DeviceManager.CollectionDeviceCommand.IsEmpty || DeviceManager.CollectionDeviceCommand[deviceId] == 255)
                    DeviceManager.CollectionDeviceCommand[deviceId] = 0;
                DeviceManager.CollectionDeviceCommand[deviceId]++;

                response.Add("commandId", DeviceManager.CollectionDeviceCommand[deviceId]);
                response.Add("command", result);

                var log = PoolConnection.LogPool.GetObject(deviceId);
                await log.SaveHistoryCommandAsync(response.ToString());
                command.CleanCommand();
            }
            return Ok(response);
        }
        
        [HttpPost]
        [ActionName("commands")]
        public async Task<IHttpActionResult> SenderCommand()
        {
            JObject json = null;
            var body = await Request.Content.ReadAsStringAsync();
            json = JObject.Parse(body);
            var request = json.SelectToken("$.deviceId").Value<String>();

            using (ISenderCommand command = new Device<ISenderCommand>(request))
            {
                command.SenderCommand(((JObject)json.SelectToken("$..command")).ToString());
            }

            return Ok();
        }
    }
}
