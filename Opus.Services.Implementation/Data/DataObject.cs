using Opus.Services.Data;
using Prism.Mvvm;
using System;

namespace Opus.Services.Implementation.Data
{
    public abstract class DataObject<T> : BindableBase where T : DataObject<T>
    {
        public Guid Id { get; set; }

        public DataObject()
        {
            Id = Guid.NewGuid();
        }

        public override bool Equals(object? obj) =>
            obj is DataObject<T> other && Equals(other);
        public bool Equals(DataObject<T> other)
        {
            return CheckEquality((T)this, (T)other);
        }
        public static bool operator ==(DataObject<T> a, DataObject<T> b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            DataObject<T> comparer = a ?? b;
            return comparer.CheckEquality((T)a, (T)b);
        }
        public static bool operator !=(DataObject<T> a, DataObject<T> b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            DataObject<T> comparer = a ?? b;
            return !comparer.CheckEquality((T)a, (T)b);
        }

        public abstract override int GetHashCode();

        protected abstract bool CheckEquality(T current, T other);
    }
}
