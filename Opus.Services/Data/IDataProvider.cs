using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Data
{
    /// <summary>
    /// Provides methods for saving, deleting and retrieving data
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Get all data of a specific type
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <returns>All data</returns>
        public List<T> GetAll<T>();
        /// <summary>
        /// Find an object.
        /// </summary>
        /// <typeparam name="T">Type of the object to find</typeparam>
        /// <param name="instance">Type instance</param>
        /// <returns>Found object. If no matching object found, return default(T).</returns>
        public T GetOne<T>(T instance);
        /// <summary>
        /// Save a data instance
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="instance">Type instance to save</param>
        /// <returns>Saved instance</returns>
        public T Save<T>(T instance) where T : IDataObject;
        /// <summary>
        /// Delete a data instance
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="instance">Type instance to delete</param>
        /// <returns>Deleted instance</returns>
        public T Delete<T>(T instance) where T : IDataObject;
        /// <summary>
        /// Clear all data of a type
        /// </summary>
        /// <typeparam name="T">Type to clear</typeparam>
        public void Clear<T>();
    }
}
