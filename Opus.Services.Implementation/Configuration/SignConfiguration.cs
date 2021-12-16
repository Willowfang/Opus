using Opus.Services.Implementation.Data;
using Opus.Services.Configuration;
using Opus.Services.Data;
using System.Threading;

namespace Opus.Services.Implementation.Configuration
{
    public class SignConfiguration : IConfiguration.Sign
    {
        private class SignatureOptions : DataObject<SignatureOptions>
        {
            public string LanguageCode { get; set; }
            public string PostFix { get; set; }

            public SignatureOptions()
            {
                LanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                PostFix = Resources.DefaultValues.Identifier;
            }
            public SignatureOptions(string languageCode)
            {
                LanguageCode = languageCode;
                PostFix = Resources.DefaultValues.Identifier;
            }

            public override int GetHashCode()
            {
                return string.IsNullOrEmpty(LanguageCode) ? this.GetHashCode() : LanguageCode.GetHashCode();
            }
            protected override bool CheckEquality(SignatureOptions current, SignatureOptions other)
            {
                return current.LanguageCode == other.LanguageCode &&
                    current.PostFix == other.PostFix;
            }
        }

        private IDataProvider provider;
        private SignatureOptions options;

        public SignConfiguration(IDataProvider dataProvider, IConfiguration.App appConfig)
        {
            provider = dataProvider;

            var current = new SignatureOptions(appConfig.GetLanguage());

            var found = provider.GetOne(current);
            if (found == null) provider.Save(current);
            options = found ?? current;
        }

        public static IConfiguration.Sign GetService(IDataProvider provider, IConfiguration.App appConfig)
            => new SignConfiguration(provider, appConfig);

        public string SignatureRemovePostfix
        {
            get => options.PostFix;
            set
            {
                var current = provider.GetOne(options);
                current.PostFix = value;
                provider.Save(current);
                options = current;
            }
        }
    }
}
