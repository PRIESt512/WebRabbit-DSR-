using System;
using System.Collections.Generic;
using System.Diagnostics;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using WebCommandDevice.ControlDevice.Pool;

namespace WebCommandDevice.ControlDevice.RabbitMQ
{
    /// <summary>
    /// Отправление и получение команд девайса
    /// </summary>
    /// <typeparam name="T">Реализация интерфейса для отправления или получения команд</typeparam>
    public sealed partial class Device<T> : IReceiveCommand, ISenderCommand where T : class
    {
        private RabbitReceiner _receiner;
        private RabbitSender _sender;

        public Device(String deviceId)
        {
            if (typeof(T) == typeof(IReceiveCommand))
                this._receiner = PoolConnection.RabbitReceiner.GetInstance(deviceId);
            else
                this._sender = PoolConnection.RabbitSender.GetInstance(deviceId);
        }

        /// <summary>
        /// Выполняет возврат использованного соединения обратно в пул
        /// </summary>
        public void Dispose()
        {
            if (this._receiner != null) PoolConnection.RabbitReceiner.ReturnToPool(ref _receiner);
            else PoolConnection.RabbitSender.ReturnToPool(ref _sender);
        }

        /// <summary>
        /// Проверка отправляемой команды
        /// </summary>
        /// <param name="command">Тело команды</param>
        /// <param name="reason">Причина отклонения команды</param>
        /// <returns></returns>
        public static Boolean CheckedCommand(JObject command, out JObject reason)
        {
            reason = new JObject();
            switch (command.SelectToken("$.commandName").ToString().ToLower())
            {
                case "delete":
                case "getinfo":
                    {
                        if (command.SelectToken("$.parameters").Value<JArray>().Count == 0)
                            return true;
                        else
                        {
                            reason.Add("ErrorCommand", Error(TypeError.ParametersError, "the parameter list must be empty"));
                            return false;
                        }
                    }
                case "upgrade":
                    {
                        List<String> parameterList = new List<String>()
                        {
                            "name", "value"
                        };
                        JToken paramToken;
                        var commandParam = command.SelectToken("$.parameters").Value<JArray>()[0].Value<JObject>();
                        if (commandParam.Count == 0 || commandParam.Count > 2)
                        {
                            reason.Add("ErrorCommand", Error(TypeError.ParametersError, "requires name and value"));
                            return false;
                        }
                        else
                        {
                            foreach (var t in parameterList)
                            {
                                if (!commandParam.TryGetValue(t, out paramToken) || paramToken.Type != JTokenType.String)
                                    reason.Add("ErrorCommand", Error(TypeError.ParametersError, String.Format("required {0} of type string", t)));
                            }
                        }
                        return reason.Count <= 0;
                    }
                case "setonoff":
                    {
                        JToken paramToken;

                        var commandParam = command.SelectToken("$.parameters").Value<JArray>()[0].Value<JObject>();
                        if (commandParam.Count == 0 || commandParam.Count > 2)
                        {
                            reason.Add("ErrorCommand", Error(TypeError.ParametersError, "requires name and value"));
                            return false;
                        }
                        else if (!commandParam.TryGetValue("name", out paramToken) ||
                            paramToken.Type != JTokenType.String ||
                            paramToken.Value<String>().ToLower() != "switchon")
                        {
                            reason.Add("ErrorCommand", Error(TypeError.ParametersError, "required name of type string"));
                        }
                        else if ((!commandParam.TryGetValue("value", out paramToken) ||
                                  paramToken.Type != JTokenType.String ||
                                  !(paramToken.Value<String>().ToLower() == "true" || paramToken.Value<String>().ToLower() == "false")))
                        {
                            reason.Add("ErrorCommand", Error(TypeError.ParametersError, "required value of type string; boolean value"));

                        }
                        return reason.Count <= 0;

                    }
                default:
                    {
                        reason.Add("ErrorCommand", Error(TypeError.CommandError));
                        return false;
                    }
            }
        }


        /// <summary>
        /// Формирование причины отмены
        /// </summary>
        /// <param name="error">Тип ошибки</param>
        /// <param name="reason">Основание для отмены</param>
        /// <returns></returns>
        private static JObject Error(TypeError error, String reason = null)
        {
            switch (error)
            {
                case TypeError.CommandError:
                    {
                        return new JObject
                            {
                                {"error", "invalid command"}
                            };
                    }
                case TypeError.ParametersError:
                    {
                        return new JObject
                            {
                                {"error", "invalid parameters"},
                                {"reason", reason}
                            };
                    }
                default:
                    return null;
            }
        }

        private enum TypeError
        {
            CommandError,
            ParametersError
        }
    }
}