using System;
using System.Net;
using System.Net.Http;
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
        public async Task<HttpResponseMessage> SenderCommand()
        {
            var jo = JObject.Parse(await this.Request.Content.ReadAsStringAsync());
            var id = jo.SelectToken("$.deviceId").Value<String>();
            JObject reason;
            if (Device<ISenderCommand>.CheckedCommand(jo.SelectToken("$.command").Value<JObject>(), out reason))
            {
                using (ISenderCommand command = new Device<ISenderCommand>(id))
                {
                    command.SenderCommand(jo.SelectToken("$..command").ToString());
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    RequestMessage = Request,
                    Content = new StringContent(reason.ToString())
                };
            }

        }
    }
}
