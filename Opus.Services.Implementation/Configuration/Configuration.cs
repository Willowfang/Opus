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
        public string? ConfigurationFile { get; set; }

        private string? languageCode;
        public string? LanguageCode
        {
            get => languageCode;
            set => SetProperty(ref languageCode, value, SaveConfiguration);
        }

        private string? extractionPrefix;
        public string? ExtractionPrefix
        {
            get => extractionPrefix;
            set => SetProperty(ref extractionPrefix, value, SaveConfiguration);
        }

        private string? extractionSuffix;
        public string? ExtractionSuffix
        {
            get => extractionSuffix;
            set => SetProperty(ref extractionSuffix, value, SaveConfiguration);
        }

        private bool extractionPrefixSuffixAsk;
        public bool ExtractionPrefixSuffixAsk
        {
            get => extractionPrefixSuffixAsk;
            set => SetProperty(ref extractionPrefixSuffixAsk, value, SaveConfiguration);
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
            if (File.Exists(configFile))
            {
                string json = File.ReadAllText(configFile);
                Configuration? config = JsonSerializer.Deserialize<Configuration>(json);
                return config ?? CreateNew(configFile);
            }
            else
            {
                return CreateNew(configFile);
            }
        }

        private static Configuration CreateNew(string configFile)
        {
            return new Configuration()
            {
                ConfigurationFile = configFile,
                LanguageCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName,
                ExtractionPrefix = null,
                ExtractionSuffix = null,
                MergeAddPageNumbers = true,
                CompositionSearchSubDirectories = true
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
