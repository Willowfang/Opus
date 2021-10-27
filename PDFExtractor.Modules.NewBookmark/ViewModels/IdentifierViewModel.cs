using PDFExtractor.Core.Base;
using PDFExtractor.Core.Singletons;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using Prism.Regions;
using System.Threading;

namespace PDFExtractor.Modules.NewBookmark.ViewModels
{
    public class IdentifierViewModel : ViewModelBase
    {
        private ICommonValues valueSingleton;
        public ICommonValues ValueSingleton
        {
            get => valueSingleton;
            set => SetProperty(ref valueSingleton, value);
        }

        public string IdentifierProxy
        {
            get => ValueSingleton.Identifier;
            set
            {
                if (languageCode == "en") Settings.InputValues.Default.Identifier_EN = value;
                else if (languageCode == "sv") Settings.InputValues.Default.Identifier_SV = value;
                else Settings.InputValues.Default.Identifier = value;

                Settings.InputValues.Default.Save();
                ValueSingleton.Identifier = value;
            }
        }

        private string languageCode;

        public IdentifierViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ICommonValues commonValues)
            : base(regionManager, eventAggregator)
        {
            languageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            ValueSingleton = commonValues;
            SetIdentifier();
        }

        private void SetIdentifier()
        {
            string value;

            if (languageCode == "en")
            {
                if (string.IsNullOrEmpty(Settings.InputValues.Default.Identifier_EN))
                    Settings.InputValues.Default.Identifier_EN = Resources.DefaultValues.Identifier;

                value = Settings.InputValues.Default.Identifier_EN;
            }

            else if (languageCode == "sv")
            {
                if (string.IsNullOrEmpty(Settings.InputValues.Default.Identifier_SV))
                    Settings.InputValues.Default.Identifier_SV = Resources.DefaultValues.Identifier;

                value = Settings.InputValues.Default.Identifier_SV;
            }

            else
            {
                if (string.IsNullOrEmpty(Settings.InputValues.Default.Identifier))
                    Settings.InputValues.Default.Identifier = Resources.DefaultValues.Identifier;

                value = Settings.InputValues.Default.Identifier;
            }

            ValueSingleton.Identifier = value;
            Settings.InputValues.Default.Save();
        }
    }
}
