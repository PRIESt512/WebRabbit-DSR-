using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCommandDevice.ControlCommand
{
    public static class DeviceManager
    {
        private static ConcurrentDictionary<String, Byte> _collectionDeviceCommand = new ConcurrentDictionary<String, Byte>();

        public static ConcurrentDictionary<String, Byte> CollectionDeviceCommand => _collectionDeviceCommand;

        public enum CommandTime
        {
            Delete = 3,
            GetInfo = 10,
            Upgrade = 10,
            SetOnOff = 8
        }
    }
}