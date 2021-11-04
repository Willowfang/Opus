using Opus.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
