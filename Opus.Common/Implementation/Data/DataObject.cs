using Prism.Mvvm;
using System.Runtime.CompilerServices;

namespace Opus.Common.Implementation.Data
{
    /// <summary>
    /// General abstract base class for storing data into the database.
    /// </summary>
    /// <typeparam name="T">Type of the data to store.</typeparam>
    public abstract class DataObject<T> : BindableBase where T : DataObject<T>
    {
        /// <summary>
        /// Id for individualization.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Create a new data object. Id will be generated automatically.
        /// </summary>
        public DataObject()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is DataObject<T> other && Equals(other);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DataObject<T> other)
        {
            return CheckEquality((T)this, (T)other);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(DataObject<T> a, DataObject<T> b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            DataObject<T> comparer = a ?? b;

            return comparer.CheckEquality(a as T, b as T);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(DataObject<T> a, DataObject<T> b)
        {
            if (a is null && b is null)
                return true;

            else if (a is null || b is null)
                return false;

            DataObject<T> comparer = a ?? b;

            return !comparer.CheckEquality(a as T, b as T);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public abstract override int GetHashCode();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        protected abstract bool CheckEquality(T? current, T? other);
    }
}
