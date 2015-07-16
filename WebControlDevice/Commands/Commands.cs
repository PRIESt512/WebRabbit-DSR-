using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace WebControlDevice.Commands
{
    public static class Commands
    {
        public static String Delete(String id)
        {
            return new JObject(
                new JProperty("SendCommandDto",
                new JObject(
                    new JProperty("deviceId", id),
                    new JProperty("command",
                    new JObject(
                        new JProperty("commandName", "delete"),
                        new JProperty("parameters",
                        new JArray(
                            new JObject(
                                new JProperty("comma", "work")))))
               )))).ToString();
        }

        public static String GetInfo(String id)
        {
            return new JObject(
               new JProperty("SendCommandDto",
               new JObject(
                   new JProperty("deviceId", id),
                   new JProperty("command",
                   new JObject(
                       new JProperty("commandName", "getInfo"),
                       new JProperty("parameters",
                       new JArray(
                           new JObject(
                               new JProperty("comma", "work")))))
               )))).ToString();
        }

        public static String Upgrade(String id, String name, String value)
        {
            return new JObject(
                new JProperty("SendCommandDto",
                new JObject(
                    new JProperty("deviceId", id),
                    new JProperty("command",
                    new JObject(
                        new JProperty("commandName", "upgrade"),
                        new JProperty("parameters",
                        new JArray(
                            new JObject(
                                new JProperty("name", name),
                                new JProperty("value", value)))))
               )))).ToString();
        }

        public static String SetOnOff(String id, Boolean onOff)
        {
            return new JObject(
               new JProperty("SendCommandDto",
               new JObject(
                   new JProperty("deviceId", id),
                   new JProperty("command",
                   new JObject(
                       new JProperty("commandName", "setOnOff"),
                       new JProperty("parameters",
                       new JArray(
                           new JObject(
                               new JProperty("name", "switchOn"),
                               new JProperty("value", new JValue(onOff))))))
              )))).ToString();
        }
    }
}