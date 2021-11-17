using System.Collections.Generic;
using System.Linq;
using Opus.Services.Data;
using LiteDB;
using System.IO;

namespace Opus.Core.ServiceImplementations.Data
{
    public class DataProviderLiteDB : IDataProvider
    {
        private readonly string databasePath;

        public DataProviderLiteDB()
        {
            databasePath = Path.Combine(Constants.FilePaths.CONFIG_DIRECTORY,
                "App" + Constants.FilePaths.CONFIG_EXTENSION);
        }

        public static IDataProvider GetService()
        {
            return new DataProviderLiteDB();
        }

        public T Save<T>(T instance) where T : IDataObject
        {
            using (var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<T>();
                var exists = collection.FindOne(x => x.Id == instance.Id);
                if (exists == null)
                    collection.Insert(instance);
                else
                    collection.Update(instance);

                return instance;
            }
        }
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
        public List<T> GetAll<T>()
        {
            List<T> found;
            using (var db = new LiteDatabase(databasePath))
            {
                found = db.GetCollection<T>().FindAll().ToList();
            }
            return found;
        }
        public T GetOne<T>(T instance)
        {
            using (var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<T>();
                var allRecords = collection.FindAll();
                return allRecords.FirstOrDefault(x => x.Equals(instance));
            }
        }
        public void Clear<T>()
        {
            using (var db = new LiteDatabase(databasePath))
            {
                db.GetCollection<T>().DeleteAll();
            }
        }
    }
}
