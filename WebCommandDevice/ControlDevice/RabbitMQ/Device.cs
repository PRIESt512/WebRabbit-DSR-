using System;
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
    }
}