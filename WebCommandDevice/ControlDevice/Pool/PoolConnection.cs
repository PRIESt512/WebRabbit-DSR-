using System;
using WebCommandDevice.ControlDevice.MongoDB;
using WebCommandDevice.ControlDevice.RabbitMQ;

namespace WebCommandDevice.ControlDevice.Pool
{
    public static class PoolConnection
    {
        public static Pool<RabbitReceiner> RabbitReceiner { get; private set; }
        public static Pool<RabbitSender> RabbitSender { get; private set; }
        public static Pool<Logging> LogPool { get; private set; }

        public static void InitializationPool()
        {
            RabbitReceiner = new Pool<RabbitReceiner>(new PoolableObjectBase<RabbitReceiner>());
            RabbitSender = new Pool<RabbitSender>(new PoolableObjectBase<RabbitSender>());
            LogPool = new Pool<Logging>(new PoolableObjectBase<Logging>());
        }

        public static void InitializationPool(Int32 minCount)
        {
            RabbitReceiner = new Pool<RabbitReceiner>(new PoolableObjectBase<RabbitReceiner>(), minCount);
            RabbitSender = new Pool<RabbitSender>(new PoolableObjectBase<RabbitSender>(), minCount);
            LogPool = new Pool<Logging>(new PoolableObjectBase<Logging>(), minCount);
        }

        public static void InitializationPool(Int32 minCount, Int32 maxCount)
        {
            RabbitReceiner = new Pool<RabbitReceiner>(new PoolableObjectBase<RabbitReceiner>(), minCount, maxCount);
            RabbitSender = new Pool<RabbitSender>(new PoolableObjectBase<RabbitSender>(), minCount, maxCount);
            LogPool = new Pool<Logging>(new PoolableObjectBase<Logging>(), minCount, maxCount);
        }
    }
}