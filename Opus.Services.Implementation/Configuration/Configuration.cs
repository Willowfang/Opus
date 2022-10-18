using Opus.Services.Configuration;
using System.Text.Json;
using System.IO;
using Prism.Mvvm;
using System.Threading;
using System;
using Opus.Services.Implementation.Logging;
using CX.LoggingLib;
using LoggingLib.Defaults;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;

namespace Opus.Services.Implementation.Configuration
{
    public class Configuration : BindableBase, IConfiguration
    {
        private const string pdfToolsLocation =
            @"C:\Program Files\Tracker Software\PDF Tools\PDFXTools.exe";

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

        private bool extractionTitleAsk;
        public bool ExtractionTitleAsk
        {
            get => extractionTitleAsk;
            set => SetProperty(ref extractionTitleAsk, value, SaveConfiguration);
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

        private bool extractionCreateZip;
        public bool ExtractionCreateZip
        {
            get => extractionCreateZip;
            set => SetProperty(ref extractionCreateZip, value, SaveConfiguration);
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

        private string? unsignedTitleTemplate;
        public string? UnsignedTitleTemplate
        {
            get => unsignedTitleTemplate;
            set => SetProperty(ref unsignedTitleTemplate, value, SaveConfiguration);
        }

        private bool workCopyFlattenRedactions;
        public bool WorkCopyFlattenRedactions
        {
            get => workCopyFlattenRedactions;
            set => SetProperty(ref workCopyFlattenRedactions, value, SaveConfiguration);
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

        public static IConfiguration Load(string configFile, ILogbook? logbook = null)
        {
            if (logbook == null)
                logbook = EmptyLogbook.Create();

            Configuration configuration;
            if (File.Exists(configFile))
            {
                string json;

                try
                {
                    json = File.ReadAllText(configFile);
                }
                catch (Exception e)
                    when (e is ArgumentException
                        || e is PathTooLongException
                        || e is IOException
                        || e is UnauthorizedAccessException
                        || e is System.Security.SecurityException
                    )
                {
                    logbook.Write(
                        $"Configuration file load failed.",
                        LogLevel.Error,
                        e,
                        nameof(Configuration)
                    );
                    throw;
                }

                Configuration? config;

                try
                {
                    config = JsonSerializer.Deserialize<Configuration>(json);
                }
                catch (ArgumentNullException e)
                {
                    logbook.Write(
                        $"Configuration file deserialization failed.",
                        LogLevel.Error,
                        e,
                        nameof(Configuration)
                    );
                    throw;
                }
                catch (JsonException e)
                {
                    logbook.Write(
                        $"Configuration file deserialization failed. Corrupted or incompatible JSON-file. Creating new configuration.",
                        LogLevel.Error,
                        e,
                        nameof(Configuration)
                    );

                    config = CreateNew(configFile);
                }

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
                ExtractionTitle = Resources.Placeholders.FileNames.Bookmark,
                MergeAddPageNumbers = true,
                CompositionSearchSubDirectories = true,
                ExtractionTitleAsk = true,
                GroupByFiles = true,
                WorkCopyFlattenRedactions = true
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
