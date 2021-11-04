using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Data
{
    /// <summary>
    /// Provides constraints on types used as data models
    /// </summary>
    public interface IDataObject
    {
        /// <summary>
        /// Unique identifier of object instance
        /// </summary>
        public int Id { get; }
    }
}
