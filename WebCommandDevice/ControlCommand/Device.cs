using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCommandDevice.ControlCommand
{
    public sealed partial class Device<T> : IReceiveCommand, ISenderCommand
    {
        private readonly RabbitReceiner _receiner;
        private readonly RabbitSender _sender;

        public Device(String deviceId)
        {
            if (typeof(T) == typeof(IReceiveCommand))
                _receiner = new RabbitReceiner(deviceId);
            else
                _sender = new RabbitSender(deviceId);
        }

        public void Dispose()
        {
            if (_receiner != null) _receiner.Dispose();
            if (_sender != null) _sender.Dispose();
        }
    }
}