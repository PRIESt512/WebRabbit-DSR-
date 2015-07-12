using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebControlDevice.Commands
{
    public static class Device
    {
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
    }
}