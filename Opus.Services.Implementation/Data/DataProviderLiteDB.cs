using System.Collections.Generic;
using System.Linq;
using Opus.Services.Data;
using LiteDB;

namespace Opus.Services.Implementation.Data
{
    public class DataProviderLiteDB : IDataProvider
    {
        private readonly string databasePath;

        public DataProviderLiteDB(string dbPath)
        {
            databasePath = dbPath;
        }

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
        public T GetOneById<T>(int id) where T : IDataObject
        {
            using (var db = new LiteDatabase(databasePath))
            {
                var collection = db.GetCollection<T>();
                var found =  collection.FindById(id);
                return found;
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
