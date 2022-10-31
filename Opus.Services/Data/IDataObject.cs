using System;

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
        public Guid Id { get; set; }
    }
}
