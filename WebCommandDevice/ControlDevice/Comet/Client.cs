using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using WebCommandDevice.ControlDevice.Pool;
using WebCommandDevice.ControlDevice.RabbitMQ;

namespace WebCommandDevice.ControlDevice.Comet
{
    public class Client : IHttpActionResult
    {
        private String _deviceId;
        private TimeSpan _connectionTimeoutSeconds;
        private Command _command;
        private DateTime time;
        HttpRequestMessage _request;

        private TaskCompletionSource<Command> _result;

        public Client(String deviceId, Int32 connectionTimeoutSeconds, HttpRequestMessage request)
        {
            _deviceId = deviceId;
            _request = request;
            _connectionTimeoutSeconds = new TimeSpan(0, 0, 0, connectionTimeoutSeconds);
            time = DateTime.UtcNow;
        }

        public string DeviceId
        {
            get { return this._deviceId; }
        }

        public TimeSpan ConnectionTimeoutSeconds
        {
            get { return this._connectionTimeoutSeconds; }
        }

        /// <summary>
        /// Определение результатов запроса команды
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private Task CompleteResult(IReceiveCommand command)
        {
            _result = new TaskCompletionSource<Command>();
            Task.Factory.StartNew(() =>
            {
                command.GetCommand(_connectionTimeoutSeconds, out _command);
                if (!_command.IsCompleted)
                    _result.SetCanceled();
                else
                {
                    _result.SetResult(_command);
                    command.CleanCommand();
                }
            }, TaskCreationOptions.LongRunning);
            return _result.Task;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            JObject body = new JObject();
            HttpResponseMessage responseMessage = null;
            try
            {
                using (IReceiveCommand command = new Device<IReceiveCommand>(_deviceId))
                {
                    await CompleteResult(command);
                }
            }
            catch (NotFoundQueue ex)
            {
                responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = _request
                };
                return responseMessage;
            }
            catch (OperationCanceledException ex)
            {
                responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = _request
                };
                return responseMessage;
            }

            if (!DeviceManager.CollectionDeviceCommand.ContainsKey(_deviceId) || DeviceManager.CollectionDeviceCommand[_deviceId] == 255)
                DeviceManager.CollectionDeviceCommand[_deviceId] = 0;
            DeviceManager.CollectionDeviceCommand[_deviceId]++;

            var jsonCommand = JObject.Load(_command.Body);
            body.Add("commandId", DeviceManager.CollectionDeviceCommand[_deviceId]);
            body.Add("command", jsonCommand);

            var log = PoolConnection.LogPool.GetInstance(_deviceId);
            await log.SaveHistoryCommandAsync(body.ToString());
            PoolConnection.LogPool.ReturnToPool(ref log);

            responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = _request,
                Content = new StringContent(body.ToString())
            };

            return responseMessage;
        }
    }
}