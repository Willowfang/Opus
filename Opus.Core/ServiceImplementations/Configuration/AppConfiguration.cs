using System.Linq;
using Opus.Services.Data;
using Opus.Services.Configuration;
using System.Threading;
using Opus.Core.ServiceImplementations.Data;

namespace Opus.Core.ServiceImplementations.Configuration
{
    public class AppConfiguration : IConfiguration.App
    {
        private class Language : DataObject<Language>
        {
            public string LanguageCode { get; set; }

            public override bool Equals(Language other)
            {
                return other != null && other.LanguageCode == LanguageCode;
            }
            public override int GetHashCode()
            {
                return LanguageCode.GetHashCode();
            }
        }

        private readonly IDataProvider provider;
        private Language CurrentLanguage;

        public AppConfiguration(IDataProvider dataProvider)
        {
            provider = dataProvider;

            var current = new Language()
            {
                LanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
            };

            var found = dataProvider.GetOne(current);
            if (found == null) dataProvider.Save(current);
            CurrentLanguage = found ?? current;
        }

        public static IConfiguration.App GetService(IDataProvider provider)
        {
            return new AppConfiguration(provider);
        }

        public static AppConfiguration GetImplementation(IDataProvider provider)
        {
            return new AppConfiguration(provider);
        }

        public void ChangeLanguage(string ISO639_1)
        {
            var found = provider.GetOne(CurrentLanguage);
            found.LanguageCode = ISO639_1;
            provider.Save(found);
        }
        public string GetLanguage()
        {
            return CurrentLanguage.LanguageCode;
        }
    }
}
