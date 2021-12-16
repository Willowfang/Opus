using Opus.Services.Data;
using Opus.Services.Configuration;
using System.Threading;
using Opus.Services.Implementation.Data;

namespace Opus.Services.Implementation.Configuration
{
    public class AppConfiguration : IConfiguration.App
    {
        private class AppOptions : DataObject<AppOptions>
        {
            public string LanguageCode { get; set; }
            public int DefaultCompositionProfile { get; set; }

            public AppOptions()
            {
                LanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                DefaultCompositionProfile = 1;
            }

            public override int GetHashCode()
            {
                return LanguageCode.GetHashCode();
            }
            protected override bool CheckEquality(AppOptions current, AppOptions other)
            {
                return current.LanguageCode == other.LanguageCode;
            }
        }

        private readonly IDataProvider provider;
        private AppOptions options;

        public AppConfiguration(IDataProvider dataProvider)
        {
            provider = dataProvider;

            var current = new AppOptions();

            var found = dataProvider.GetOneById<AppOptions>(1);
            if (found == null) dataProvider.Save(current);
            options = found ?? current;
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
            var found = provider.GetOneById<AppOptions>(1);
            found.LanguageCode = ISO639_1;
            provider.Save(found);
        }
        public string GetLanguage()
        {
            return options.LanguageCode;
        }
    }
}
