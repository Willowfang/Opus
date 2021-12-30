using Opus.Services.Configuration;
using Opus.Services.Data;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Data
{
    public class SignatureOptions : ISignatureOptions
    {
        private class SignatureOptionsDTO : DataObject<SignatureOptionsDTO>, IDataObject
        {
            public string? LanguageCode { get; set; }
            public string? Suffix { get; set; }

            public override int GetHashCode()
            {
                return string.IsNullOrEmpty(LanguageCode) ? this.GetHashCode() : LanguageCode.GetHashCode();
            }
            protected override bool CheckEquality(SignatureOptionsDTO current, SignatureOptionsDTO other)
            {
                return current.LanguageCode == other.LanguageCode;
            }
        }

        private IDataProvider dataProvider;
        private SignatureOptionsDTO options;

        public string? Suffix
        {
            get => options.Suffix;
            set
            {
                options.Suffix = value;
                SaveOptions();
            }
        }

        public SignatureOptions(IDataProvider dataProvider, IConfiguration configuration)
        {
            this.dataProvider = dataProvider;

            SignatureOptionsDTO created = CreateNew(configuration.LanguageCode);
            SignatureOptionsDTO current = dataProvider.GetOne(created);

            if (current == null)
            {
                options = dataProvider.Save(created);
            }
            else
            {
                options = current;
            }
        }

        private SignatureOptionsDTO CreateNew(string languageCode)
        {
            return new SignatureOptionsDTO()
            {
                LanguageCode = languageCode,
                Suffix = Resources.DefaultValues.DefaultValues.UnsignedSuffix
            };
        }

        private void SaveOptions()
        {
            dataProvider.Save(options);
        }
    }
}
