using Opus.Services.Configuration;
using System.Text.Json;
using System.IO;
using Prism.Mvvm;
using System.Threading;
using System;

namespace Opus.Services.Implementation.Configuration
{
    public class Configuration : BindableBase, IConfiguration
    {
        private const string pdfToolsLocation = @"C:\Program Files\Tracker Software\PDF Tools\PDFXTools.exe";

        public string? ConfigurationFile { get; set; }

        private string? languageCode;
        public string? LanguageCode
        {
            get => languageCode;
            set => SetProperty(ref languageCode, value, SaveConfiguration);
        }

        private string? extractionTitle;
        public string? ExtractionTitle
        {
            get => extractionTitle;
            set => SetProperty(ref extractionTitle, value, SaveConfiguration);
        }

        private string? extractionSuffix;
        public string? ExtractionSuffix
        {
            get => extractionSuffix;
            set => SetProperty(ref extractionSuffix, value, SaveConfiguration);
        }

        private bool extractionPrefixSuffixAsk;
        public bool ExtractionTitleAsk
        {
            get => extractionPrefixSuffixAsk;
            set => SetProperty(ref extractionPrefixSuffixAsk, value, SaveConfiguration);
        }

        private bool extractionConvertPdfA;
        public bool ExtractionConvertPdfA
        {
            get => extractionConvertPdfA;
            set => SetProperty(ref extractionConvertPdfA, value, SaveConfiguration);
        }

        private bool extractionPdfADisabled;
        public bool ExtractionPdfADisabled
        {
            get => extractionPdfADisabled;
            set => SetProperty(ref extractionPdfADisabled, value);
        }

        private int annotations;
        public int Annotations
        {
            get => annotations;
            set => SetProperty(ref annotations, value, SaveConfiguration);
        }

        private bool groupByFiles;
        public bool GroupByFiles
        {
            get => groupByFiles;
            set => SetProperty(ref groupByFiles, value, SaveConfiguration);
        }

        private bool mergeAddPageNumbers;
        public bool MergeAddPageNumbers
        {
            get => mergeAddPageNumbers;
            set => SetProperty(ref mergeAddPageNumbers, value, SaveConfiguration);
        }

        private bool compositionSearchSubDirectories;
        public bool CompositionSearchSubDirectories
        {
            get => compositionSearchSubDirectories;
            set => SetProperty(ref compositionSearchSubDirectories, value, SaveConfiguration);
        }

        private bool compositionDeleteConverted;
        public bool CompositionDeleteConverted
        {
            get => compositionDeleteConverted;
            set => SetProperty(ref compositionDeleteConverted, value, SaveConfiguration);
        }

        private Guid defaultProfile;
        public Guid DefaultProfile
        {
            get => defaultProfile;
            set => SetProperty(ref defaultProfile, value, SaveConfiguration);
        }

        public Configuration() { }

        public static IConfiguration Load(string configFile)
        {
            Configuration configuration;
            if (File.Exists(configFile))
            {
                string json = File.ReadAllText(configFile);
                Configuration? config = JsonSerializer.Deserialize<Configuration>(json);
                configuration = config ?? CreateNew(configFile);
            }
            else
            {
                configuration = CreateNew(configFile);
            }

            if (File.Exists(pdfToolsLocation) == false)
            {
                configuration.ExtractionConvertPdfA = false;
                configuration.ExtractionPdfADisabled = true;
            }

            return configuration;
        }

        private static Configuration CreateNew(string configFile)
        {
            return new Configuration()
            {
                ConfigurationFile = configFile,
                LanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName,
                ExtractionTitle = Resources.DefaultValues.DefaultValues.Bookmark,
                MergeAddPageNumbers = true,
                CompositionSearchSubDirectories = true,
                ExtractionTitleAsk = true,
                GroupByFiles = true
            };
        }

        private void SaveConfiguration()
        {
            if (!string.IsNullOrEmpty(ConfigurationFile))
            {
                File.WriteAllText(ConfigurationFile, JsonSerializer.Serialize(this));
            }
        }
    }
}
