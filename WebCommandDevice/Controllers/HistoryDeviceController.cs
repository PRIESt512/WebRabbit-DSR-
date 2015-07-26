using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using WebCommandDevice.ControlDevice.Pool;

namespace WebCommandDevice.Controllers
{
    public class HistoryDeviceController : ApiController
    {
        [HttpGet]
        [ActionName("history")]
        public async Task<IHttpActionResult> HistoryDevice(String deviceId)
        {
            var log = PoolConnection.LogPool.GetObject(deviceId);
            List<String> response = null;
            try
            {
                response = await log.GetHistoryCommandAsync();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
            PoolConnection.LogPool.ReturnObject(ref log);
            return Ok(response);
        }
    }
}
