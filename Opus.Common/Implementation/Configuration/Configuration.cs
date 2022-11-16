using Opus.Common.Services.Configuration;
using System.Text.Json;
using System.IO;
using Prism.Mvvm;
using WF.LoggingLib;
using WF.LoggingLib.Defaults;

namespace Opus.Common.Implementation.Configuration
{
    /// <summary>
    /// Configuration implementation for <see cref="IConfiguration"/> service.
    /// </summary>
    public class Configuration : BindableBase, IConfiguration
    {
        /// <summary>
        /// Location of pdf-tools installation (for pdf/a conversion).
        /// </summary>
        private const string pdfToolsLocation =
            @"C:\Program Files\Tracker Software\PDF Tools\PDFXTools.exe";

        /// <summary>
        /// Configuration file location as path.
        /// </summary>
        public string? ConfigurationFile { get; set; }

        private string? languageCode;

        /// <summary>
        /// Current language code (two letters, e.g. "fi").
        /// </summary>
        public string? LanguageCode
        {
            get => languageCode;
            set => SetProperty(ref languageCode, value, SaveConfiguration);
        }

        private string? extractionTitle;

        /// <summary>
        /// Title template for extracted bookmarks.
        /// </summary>
        public string? ExtractionTitle
        {
            get => extractionTitle;
            set => SetProperty(ref extractionTitle, value, SaveConfiguration);
        }

        private bool extractionTitleAsk;

        /// <summary>
        /// If true, always ask for the template.
        /// </summary>
        public bool ExtractionTitleAsk
        {
            get => extractionTitleAsk;
            set => SetProperty(ref extractionTitleAsk, value, SaveConfiguration);
        }

        private bool extractionConvertPdfA;

        /// <summary>
        /// If true, convert extracted files to pdf/a.
        /// </summary>
        public bool ExtractionConvertPdfA
        {
            get => extractionConvertPdfA;
            set => SetProperty(ref extractionConvertPdfA, value, SaveConfiguration);
        }

        private bool extractionPdfADisabled;

        /// <summary>
        /// If true, pdf/a-conversion is not allowed.
        /// </summary>
        public bool ExtractionPdfADisabled
        {
            get => extractionPdfADisabled;
            set => SetProperty(ref extractionPdfADisabled, value);
        }

        private bool extractionCreateZip;

        /// <summary>
        /// Compress extracted bookmarks into a zip-file.
        /// </summary>
        public bool ExtractionCreateZip
        {
            get => extractionCreateZip;
            set => SetProperty(ref extractionCreateZip, value, SaveConfiguration);
        }

        private bool extractionOpenDestinationAfterComplete;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool ExtractionOpenDestinationAfterComplete
        {
            get => extractionOpenDestinationAfterComplete;
            set => SetProperty(ref extractionOpenDestinationAfterComplete, value, SaveConfiguration);
        }

        private int annotations;

        /// <summary>
        /// Option for dealing with annotations.
        /// </summary>
        public int Annotations
        {
            get => annotations;
            set => SetProperty(ref annotations, value, SaveConfiguration);
        }

        private bool groupByFiles;

        /// <summary>
        /// If true, group bookmarks by file when extracting into a single file.
        /// </summary>
        public bool GroupByFiles
        {
            get => groupByFiles;
            set => SetProperty(ref groupByFiles, value, SaveConfiguration);
        }

        private string? unsignedTitleTemplate;

        /// <summary>
        /// Template for work copy file names.
        /// </summary>
        public string? UnsignedTitleTemplate
        {
            get => unsignedTitleTemplate;
            set => SetProperty(ref unsignedTitleTemplate, value, SaveConfiguration);
        }

        private bool workCopyFlattenRedactions;

        /// <summary>
        /// If true, flatten redactions (make them red rectangles) when creating work copies.
        /// </summary>
        public bool WorkCopyFlattenRedactions
        {
            get => workCopyFlattenRedactions;
            set => SetProperty(ref workCopyFlattenRedactions, value, SaveConfiguration);
        }

        private bool mergeAddPageNumbers;

        /// <summary>
        /// If true, add page numbers when merging documents.
        /// </summary>
        public bool MergeAddPageNumbers
        {
            get => mergeAddPageNumbers;
            set => SetProperty(ref mergeAddPageNumbers, value, SaveConfiguration);
        }

        private bool compositionSearchSubDirectories;

        /// <summary>
        /// If true, include subdirectories in the search when looking for matching files.
        /// </summary>
        public bool CompositionSearchSubDirectories
        {
            get => compositionSearchSubDirectories;
            set => SetProperty(ref compositionSearchSubDirectories, value, SaveConfiguration);
        }

        private bool compositionDeleteConverted;

        /// <summary>
        /// If true, delete converted files after composition is done.
        /// </summary>
        public bool CompositionDeleteConverted
        {
            get => compositionDeleteConverted;
            set => SetProperty(ref compositionDeleteConverted, value, SaveConfiguration);
        }

        private Guid defaultProfile;

        /// <summary>
        /// Default composition profile.
        /// </summary>
        public Guid DefaultProfile
        {
            get => defaultProfile;
            set => SetProperty(ref defaultProfile, value, SaveConfiguration);
        }

        private int loggingLevel;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int LoggingLevel
        {
            get => loggingLevel;
            set => SetProperty(ref loggingLevel, value, SaveConfiguration);
        }

        /// <summary>
        /// Create new configuration instance.
        /// </summary>
        public Configuration() { }

        /// <summary>
        /// Load configuration from a JSON file.
        /// </summary>
        /// <param name="configFile">Path of the file to load.</param>
        /// <param name="logbook">Logging service.</param>
        /// <returns>Configuration service.</returns>
        public static IConfiguration Load(string configFile, ILogbook? logbook = null)
        {
            logbook ??= EmptyLogbook.Create();

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

        /// <summary>
        /// Create new configuration instance.
        /// </summary>
        /// <param name="configFile">Location to save config at.</param>
        /// <returns>Configuration instance.</returns>
        protected static Configuration CreateNew(string configFile)
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
                WorkCopyFlattenRedactions = true,
                UnsignedTitleTemplate = Resources.DefaultValues.DefaultValues.UnsignedTemplate,
                ExtractionOpenDestinationAfterComplete = true,
                LoggingLevel = (int)LogLevel.Information
            };
        }

        /// <summary>
        /// Save configuration to a file (as JSON).
        /// </summary>
        private void SaveConfiguration()
        {
            if (!string.IsNullOrEmpty(ConfigurationFile))
            {
                File.WriteAllText(ConfigurationFile, JsonSerializer.Serialize(this));
            }
        }
    }
}
