using Opus.Services.Data;

namespace Opus.Core.ServiceImplementations.Data
{
    public abstract class DataObject<T> : IDataObject
    {
        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is T)
                return Equals((T)obj);

            return false;
        }
        public abstract bool Equals(T other);

        public abstract override int GetHashCode();
    }
}
