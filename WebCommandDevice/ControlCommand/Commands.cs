using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace WebCommandDevice.ControlCommand
{
    public static class Commands
    {
        #region Классы для сериализации
        private class Device
        {
            public Int16 commandId;
            public String command;
        }

        private class GetResponseDto
        {
            public Device GetCommandResponseDto;

            public String GetJsonGetCommandResponse()
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
        }
        #endregion

        public static String GetResponse(String command, Int16 commandId)
        {
            var device = new Device
            {
                commandId = commandId,
                command = command
            };

            var response = new GetResponseDto
            {
                GetCommandResponseDto = device
            };

            return response.GetJsonGetCommandResponse();
        }
    }
}