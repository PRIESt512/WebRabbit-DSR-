using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCommandDevice.ControlCommand
{
    /// <summary>
    /// Класс управления командами для всех девайсов
    /// </summary>
    public static class DeviceManager
    {
        private static ConcurrentDictionary<String, Byte> _collectionDeviceCommand = new ConcurrentDictionary<String, Byte>();

        public static ConcurrentDictionary<String, Byte> CollectionDeviceCommand
        {
            get { return _collectionDeviceCommand; }
        }

        /// <summary>
        /// Таймаут конкретной команды
        /// </summary>
        public enum CommandTime
        {
            Delete = 15,
            GetInfo = 30,
            Upgrade = 30,
            SetOnOff = 20
        }
    }
}