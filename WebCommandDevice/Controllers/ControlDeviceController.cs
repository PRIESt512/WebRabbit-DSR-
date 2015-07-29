using System;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using WebCommandDevice.ControlDevice.Comet;
using WebCommandDevice.ControlDevice.RabbitMQ;


namespace WebCommandDevice.Controllers
{
    public class ControlDeviceController : ApiController
    {
        [HttpGet]
        [ActionName("commands")]
        public IHttpActionResult CheckCommand(String deviceId, Int32 timeout)
        {
            Client client = new Client(deviceId, timeout, Request);
            return client;
        }

        [HttpPost]
        [ActionName("commands")]
        public async Task<IHttpActionResult> SenderCommand()
        {
            var jo = JObject.Parse(await this.Request.Content.ReadAsStringAsync());
            var id = jo.SelectToken("$.deviceId").Value<String>();
            using (ISenderCommand command = new Device<ISenderCommand>(id))
            {
                command.SenderCommand(((JObject)jo.SelectToken("$..command")).ToString());
            }
            return Ok();
        }
    }
}
