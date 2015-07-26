using System;
using System.Collections.Concurrent;

namespace WebCommandDevice.ControlDevice
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
    }
}