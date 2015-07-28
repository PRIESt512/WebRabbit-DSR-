using System;
using System.Collections.Concurrent;

namespace WebCommandDevice.ControlDevice.Pool
{
    /// <summary>
    /// Интерфейс объекта в пуле
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Установление состояния перед использованием
        /// </summary>
        /// <param name="deviceId"></param>
        void InstallationState(String deviceId);

        /// <summary>
        /// Сброс состояния объекта
        /// </summary>
        void ResetState();
    }

    /// <summary>
    /// Интерфейс создания объекта в пуле
    /// </summary>
    /// <typeparam name="T">
    /// Тип создаваего объекта
    /// </typeparam>
    public interface IPoolObjectCreator<T>
    {
        /// <summary>
        /// Создает новый объект в пуле
        /// </summary>
        /// <returns>Возвращает объект</returns>
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
        private readonly ConcurrentBag<T> _pool = new ConcurrentBag<T>();

        private readonly Int32 _maxCount;
        private readonly Int32 _minCount;

        private readonly IPoolObjectCreator<T> _objectCreator;

        /// <summary>Количество объектов в пуле</summary>
        public int Count { get { return _pool.Count; } }

        #region Конструкторы
        public Pool(IPoolObjectCreator<T> creator)
        {
            if (creator == null)
                throw new ArgumentNullException("creator can't be null");

            this._objectCreator = creator;
            this._maxCount = 0;
            this._minCount = 0;
        }

        public Pool(IPoolObjectCreator<T> creator, Int32 minCount) : this(creator)
        {
            _minCount = minCount;
            for (int i = 0; i < minCount; i++)
            {
                var obj = this._objectCreator.Create();

                this._pool.Add(obj);
            }
        }

        public Pool(IPoolObjectCreator<T> creator, Int32 minCount, Int32 maxCount) : this(creator, minCount)
        {
            if (minCount > maxCount)
                throw new ArgumentException("minCount more maxCount");

            this._maxCount = maxCount;
        }
        #endregion

        /// <summary>Возвращает или создает новый обект в пуле</summary>
        /// <returns>Объекта типа T</returns>
        public T GetInstance(String deviceId)
        {
            T obj;
            if (this._pool.TryTake(out obj))
            {
                obj.InstallationState(deviceId);
                return obj;
            }

            obj = _objectCreator.Create();
            obj.InstallationState(deviceId);
            return obj;
        }

        /// <summary>
        /// Возвращает указанный объект в пул
        /// </summary>
        /// <param name="obj">Возвращаемый объект</param>
        public void ReturnToPool(ref T obj)
        {
            obj.ResetState();
            this._pool.Add(obj);
            obj = null;
        }

        /// <summary>
        /// Вызывает уничтожение всех объектов в пуле
        /// </summary>
        public void Dispose()
        {
            T obj;
            var count = _pool.Count;
            for (int i = 0; i < count; i++)
            {
                this._pool.TryTake(out obj);
                obj.Dispose();
            }
        }
    }
}