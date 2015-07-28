using System;
using System.Collections.Generic;
using System.Web.Http;

namespace WebCommandDevice.Controllers
{
    public class SimpleDeviceController : ApiController
    {
        Dictionary<String, String> _command = new Dictionary<String, String>();

        public IHttpActionResult CheckCommand(String deviceId)
        {
            return Ok(_command[deviceId]);
        }

        public IHttpActionResult SenderCommand(String deviceId, String command)
        {
            _command[deviceId] = command;

            return Ok();
        }
    }
}
