using System.Collections.Generic;
using System.Linq;
using Opus.Services.Data;
using LiteDB;

namespace Opus.Services.Implementation.Data
{
    /// <summary>
    /// Default implementation for <see cref="IDataProvider"/>. Accesses a local LiteDB database.
    /// </summary>
    public class DataProviderLiteDB : IDataProvider
    {
        private readonly string databasePath;

        /// <summary>
        /// Create a new data provider instance.
        /// </summary>
        /// <param name="dbPath">Filepath of the database.</param>
        public DataProviderLiteDB(string dbPath)
        {
            databasePath = dbPath;
        }

        /// <summary>
        /// Save an instance into the database.
        /// </summary>
        /// <typeparam name="T">Type of the data to save.</typeparam>
        /// <param name="instance">Instance to save.</param>
        /// <returns>The saved instance.</returns>
        public T Save<T>(T instance) where T : IDataObject
        {
            using (var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<T>();
                /* var exists = collection.FindOne(x => x.Id == instance.Id);
                if (exists == null)
                    collection.Insert(instance);
                else
                    collection.Update(instance);*/

                collection.Upsert(instance);

                return instance;
            }
        }

        /// <summary>
        /// Remove an instance from the database.
        /// </summary>
        /// <typeparam name="T">Type of the data to remove.</typeparam>
        /// <param name="instance">Instance to remove.</param>
        /// <returns>The removed instance.</returns>
        public T Delete<T>(T instance) where T : IDataObject
        {
            T found;
            using (var db = new LiteDatabase(databasePath))
            {
                db.GetCollection<T>().Delete(instance.Id);
                found = instance;
            }
            return found;
        }

        /// <summary>
        /// Find all instances of data from the database.
        /// </summary>
        /// <typeparam name="T">Type of the data to retrieve.</typeparam>
        /// <returns>All found instances.</returns>
        public List<T> GetAll<T>()
        {
            List<T> found;
            using (var db = new LiteDatabase(databasePath))
            {
                found = db.GetCollection<T>().FindAll().ToList();
            }
            return found;
        }

        /// <summary>
        /// Find a particular instance of data.
        /// </summary>
        /// <typeparam name="T">Type of the stored data.</typeparam>
        /// <param name="instance">Instance to find.</param>
        /// <returns>Found instance or null, if none found.</returns>
        public T? GetOne<T>(T instance)
        {
            using (var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<T>();
                var allRecords = collection.FindAll();
                return allRecords.FirstOrDefault(x => x.Equals(instance));
            }
        }

        /// <summary>
        /// Find a particular instance of data by its id.
        /// </summary>
        /// <typeparam name="T">Type of the stored data.</typeparam>
        /// <param name="id">Id of the instance to find.</param>
        /// <returns>Found instance or null, if none found.</returns>
        public T GetOneById<T>(int id) where T : IDataObject
        {
            using (var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<T>();
                var found = collection.FindById(id);
                return found;
            }
        }

        /// <summary>
        /// Clear the whole database of all instances.
        /// </summary>
        /// <typeparam name="T">Type of the instances to clear.</typeparam>
        public void Clear<T>()
        {
            using (var db = new LiteDatabase(databasePath))
            {
                db.GetCollection<T>().DeleteAll();
            }
        }
    }
}
