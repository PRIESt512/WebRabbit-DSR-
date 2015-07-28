using System;
using System.IO;
using Newtonsoft.Json;

namespace WebCommandDevice.ControlDevice.Comet
{
    public class Command
    {
        public Command(String body)
        {
            Body = new JsonTextReader(new StringReader(body));
        }

        public Command(Boolean flag)
        {
            isCompleted = flag;
        }

        private Boolean isCompleted = true;
        public Boolean IsCompleted { get { return isCompleted; } }

        public JsonTextReader Body { get; private set; }

    }
}