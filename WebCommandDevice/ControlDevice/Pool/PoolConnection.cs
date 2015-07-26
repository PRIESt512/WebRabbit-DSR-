using System;
using WebCommandDevice.ControlDevice.MongoDB;
using WebCommandDevice.ControlDevice.RabbitMQ;

namespace WebCommandDevice.ControlDevice.Pool
{
    public static class PoolConnection
    {
        public static ObjectPool<RabbitReceiner> RabbitReceiner { get; private set; }
        public static ObjectPool<RabbitSender> RabbitSender { get; private set; }
        public static ObjectPool<Logging> LogPool { get; private set; }

        public static void InitializationPool()
        {
            RabbitReceiner = new ObjectPool<RabbitReceiner>(new ObjectCreator<RabbitReceiner>());
            RabbitSender = new ObjectPool<RabbitSender>(new ObjectCreator<RabbitSender>());
            LogPool = new ObjectPool<Logging>(new ObjectCreator<Logging>());
        }

        public static void InitializationPool(Int32 minCount)
        {
            RabbitReceiner = new ObjectPool<RabbitReceiner>(new ObjectCreator<RabbitReceiner>(), minCount);
            RabbitSender = new ObjectPool<RabbitSender>(new ObjectCreator<RabbitSender>(), minCount);
            LogPool = new ObjectPool<Logging>(new ObjectCreator<Logging>(), minCount);
        }

        public static void InitializationPool(Int32 minCount, Int32 maxCount)
        {
            RabbitReceiner = new ObjectPool<RabbitReceiner>(new ObjectCreator<RabbitReceiner>(), minCount, maxCount);
            RabbitSender = new ObjectPool<RabbitSender>(new ObjectCreator<RabbitSender>(), minCount, maxCount);
            LogPool = new ObjectPool<Logging>(new ObjectCreator<Logging>(), minCount, maxCount);
        }
    }
}