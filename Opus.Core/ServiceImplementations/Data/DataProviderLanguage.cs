using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opus.Services.Data;
using System.Text.Json;

namespace Opus.Core.ServiceImplementations.Data
{
    public class DataProviderLanguage : IDataProvider
    {
        private readonly string LanguageFile;

        public DataProviderLanguage()
        {
            LanguageFile = Path.Combine(Constants.FilePaths.CONFIG_DIRECTORY, 
                "Language" + Constants.FilePaths.CONFIG_EXTENSION);
        }

        public static IDataProvider GetService()
        {
            return new DataProviderLanguage();
        }

        public void Clear<T>()
        {
            if (File.Exists(LanguageFile)) File.Delete(LanguageFile);
        }

        public T Delete<T>(T instance) where T : IDataObject
        {
            if (File.Exists(LanguageFile)) File.Delete(LanguageFile);
            return instance;
        }

        public List<T> GetAll<T>()
        {
            var dummy = new List<T>();
            if (File.Exists(LanguageFile)) dummy.Add(JsonSerializer.Deserialize<T>(File.ReadAllText(LanguageFile)));
            return dummy;
        }

        public T GetOne<T>(T instance)
        {
            return File.Exists(LanguageFile) ? JsonSerializer.Deserialize<T>(File.ReadAllText(LanguageFile)) : default(T);
        }

        public T Save<T>(T instance) where T : IDataObject
        {
            File.WriteAllText(LanguageFile, JsonSerializer.Serialize(instance));
            return instance;
        }
    }
}
