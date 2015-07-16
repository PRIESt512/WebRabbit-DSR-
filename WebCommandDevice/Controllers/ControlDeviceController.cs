﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using WebCommandDevice.ControlCommand;

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
            JObject response;
            using (IReceiveCommand command = new Device<IReceiveCommand>(deviceId))
            {
                result = await command.GetCommandAsync(_timeout);

                if (result.Equals("NotFound")) return NotFound();

                if (DeviceManager.CollectionDeviceCommand.IsEmpty || DeviceManager.CollectionDeviceCommand[deviceId] == 255)
                    DeviceManager.CollectionDeviceCommand[deviceId] = 0;
                DeviceManager.CollectionDeviceCommand[deviceId]++;
                
                response = new JObject(
                    new JProperty("GetCommandResponseDto",
                    new JObject(
                        new JProperty("commandId", DeviceManager.CollectionDeviceCommand[deviceId]),
                        new JProperty("command", result))));

                command.CleanCommand();
            }
            return Ok(response);
        }

        [HttpPost]
        [ActionName("commands")]
        public async Task<IHttpActionResult> SendCommand()
        {
            JObject json = null;
            var request = await this.Request.Content.ReadAsStringAsync();

            json = JObject.Parse(request);

            using (ISenderCommand command = new Device<ISenderCommand>(json.SelectToken("$..deviceId").Value<String>()))
            {
                command.SenderCommand(((JObject)json.SelectToken("$..command")).ToString());
            }

            return Ok();
        }
    }
}
