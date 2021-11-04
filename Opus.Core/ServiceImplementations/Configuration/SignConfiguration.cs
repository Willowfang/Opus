using Opus.Core.ServiceImplementations.Data;
using Opus.Services.Configuration;
using Opus.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.ServiceImplementations.Configuration
{
    public class SignConfiguration : IConfiguration.Sign
    {
        private class SignaturePostFix : DataObject<SignaturePostFix>
        {
            public string LanguageCode { get; set; }
            public string PostFix { get; set; }

            public override bool Equals(SignaturePostFix other)
            {
                return other != null && LanguageCode == other.LanguageCode;
            }
            public override int GetHashCode()
            {
                return LanguageCode.GetHashCode();
            }
        }

        private IDataProvider provider;
        private SignaturePostFix CurrentPostFix;

        public SignConfiguration(IDataProvider dataProvider, IConfiguration.App appConfig)
        {
            provider = dataProvider;

            var current = new SignaturePostFix() 
            { 
                LanguageCode = appConfig.GetLanguage(),
                PostFix = Resources.DefaultValues.Identifier
            };

            var found = provider.GetOne(current);
            if (found == null) provider.Save(current);
            CurrentPostFix = found ?? current;
        }

        public static IConfiguration.Sign GetService(IDataProvider provider, IConfiguration.App appConfig)
            => new SignConfiguration(provider, appConfig);

        public string SignatureRemovePostfix
        {
            get => CurrentPostFix.PostFix;
            set
            {
                var current = provider.GetOne(CurrentPostFix);
                current.PostFix = value;
                provider.Save(current);
                CurrentPostFix = current;
            }
        }
    }
}
