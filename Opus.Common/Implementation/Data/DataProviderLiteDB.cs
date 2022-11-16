using LiteDB;
using Opus.Common.Logging;
using Opus.Common.Services.Data;
using WF.LoggingLib;

namespace Opus.Common.Implementation.Data
{
    /// <summary>
    /// Default implementation for <see cref="IDataProvider"/>. Accesses a local LiteDB database.
    /// </summary>
    public class DataProviderLiteDB : LoggingCapable<DataProviderLiteDB>, IDataProvider
    {
        private readonly string databasePath;

        /// <summary>
        /// Create a new data provider instance.
        /// </summary>
        /// <param name="dbPath">Filepath of the database.</param>
        /// <param name="logbook">Logging service.</param>
        public DataProviderLiteDB(string dbPath, ILogbook logbook) : base(logbook)
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
            logbook.Write($"Saving to database.", LogLevel.Debug);

            using (var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<T>();

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
            logbook.Write($"Deleting from database.", LogLevel.Debug);

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
            logbook.Write($"Retrieving all records from the database.", LogLevel.Debug);

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
            logbook.Write($"Retrieving one record from the database.", LogLevel.Debug);

            using (var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<T>();
                var allRecords = collection.FindAll();
                return allRecords.FirstOrDefault(x => x != null && x.Equals(instance));
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
            logbook.Write($"Retrieving one record from the database by id.", LogLevel.Debug);

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
            logbook.Write($"Clearing database table.", LogLevel.Debug);

            using (var db = new LiteDatabase(databasePath))
            {
                db.GetCollection<T>().DeleteAll();
            }
        }
    }
}
