using System;
using System.Collections.Concurrent;

namespace WebCommandDevice.ControlDevice.Pool
{
    public interface IPoolable
    {
        void InstallationState(String deviceId);
        void ResetState();
    }

    /// <summary>The pool object creator interface.</summary>
    /// <typeparam name="T">Type of the objects to create.</typeparam>
    public interface IPoolObjectCreator<T>
    {
        /// <summary>Creates new object for a pool.</summary>
        /// <returns>The object.</returns>
        T Create();
    }

    public class PoolableObjectBase<T> : IPoolObjectCreator<T> where T : class, new()
    {
        T IPoolObjectCreator<T>.Create()
        {
            return new T();
        }
    }

    public class Pool<T> : IDisposable where T : class, IPoolable, IDisposable
    {
        /// <summary>Object container. ConcurrentBag is tread-safe class.</summary>
        private readonly ConcurrentBag<T> _pool = new ConcurrentBag<T>();

        private readonly Int32 _maxCount;
        private readonly Int32 _minCount;

        /// <summary>Object creator interface.</summary>
        private readonly IPoolObjectCreator<T> _objectCreator;

        /// <summary>Total instances.</summary>
        public int Count { get { return _pool.Count; } }

        #region Конструкторы
        public Pool(IPoolObjectCreator<T> creator)
        {
            if (creator == null)
                throw new ArgumentNullException("creator can't be null");

            _objectCreator = creator;
            _maxCount = 0;
            _minCount = 0;
        }

        public Pool(IPoolObjectCreator<T> creator, Int32 minCount) : this(creator)
        {
            _minCount = minCount;
            for (int i = 0; i < minCount; i++)
            {
                var obj = _objectCreator.Create();

                _pool.Add(obj);
            }
        }

        public Pool(IPoolObjectCreator<T> creator, Int32 minCount, Int32 maxCount) : this(creator, minCount)
        {
            if (minCount > maxCount)
                throw new ArgumentException("minCount more maxCount");

            _maxCount = maxCount;
        }
        #endregion

        /// <summary>Gets an object from the pool.</summary>
        /// <returns>An object.</returns>
        public T GetInstance(String deviceId)
        {
            T obj;
            if (_pool.TryTake(out obj))
            {
                obj.InstallationState(deviceId);
                return obj;
            }

            obj = _objectCreator.Create();
            obj.InstallationState(deviceId);
            return obj;
        }

        /// <summary>Returns the specified object to the pool.</summary>
        /// <param name="obj">The object to return.</param>
        public void ReturnToPool(ref T obj)
        {
            obj.ResetState();
            _pool.Add(obj);
            obj = null;
        }

        public void Dispose()
        {
            T obj;
            var count = _pool.Count;
            for (int i = 0; i < count; i++)
            {
                _pool.TryTake(out obj);
                obj.Dispose();
            }
        }
    }
}