using Opus.Common.Services.Data.Composition;

namespace Opus.Common.Services.Configuration
{
    /// <summary>
    /// Application configuration service
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Application language in ISO639-1 format
        /// </summary>
        public string? LanguageCode { get; set; }

        /// <summary>
        /// Title for extraction
        /// </summary>
        public string? ExtractionTitle { get; set; }

        /// <summary>
        /// If true, title will be asked every time
        /// an extraction is done
        /// </summary>
        public bool ExtractionTitleAsk { get; set; }

        /// <summary>
        /// Convert resulting pdf-files to pdf/a
        /// </summary>
        public bool ExtractionConvertPdfA { get; set; }

        /// <summary>
        /// Indicates if conversion to pdf/a is possible
        /// </summary>
        public bool ExtractionPdfADisabled { get; set; }

        /// <summary>
        ///  Indicates whether extracted files should be compressed
        ///  into a zip file instead of separate pdfs
        /// </summary>
        public bool ExtractionCreateZip { get; set; }

        /// <summary>
        /// If true, open directory or file after extraction is complete.
        /// </summary>
        public bool ExtractionOpenDestinationAfterComplete { get; set; }

        /// <summary>
        /// Annotation handling option
        /// </summary>
        public int Annotations { get; set; }

        /// <summary>
        /// Indicates whether individual bookmarks are grouped by source files
        /// </summary>
        public bool GroupByFiles { get; set; }

        /// <summary>
        /// Title for files that have had their signatures removed
        /// </summary>
        public string? UnsignedTitleTemplate { get; set; }

        /// <summary>
        /// Flatten redactions when creating work copies
        /// </summary>
        public bool WorkCopyFlattenRedactions { get; set; }

        /// <summary>
        /// If true, page numbers will be added to a merged document
        /// </summary>
        public bool MergeAddPageNumbers { get; set; }

        /// <summary>
        /// Include subdirectories when searching for composition files
        /// </summary>
        public bool CompositionSearchSubDirectories { get; set; }

        /// <summary>
        /// If true, files converted to pdf during composition will be deleted. Said
        /// files will be deleted when cancelling, regardless of the value of this property.
        /// </summary>
        public bool CompositionDeleteConverted { get; set; }

        /// <summary>
        /// The <see cref="ICompositionProfile"/> to select by default
        /// </summary>
        public Guid DefaultProfile { get; set; }

        /// <summary>
        /// Logging level for the logger.
        /// </summary>
        public int LoggingLevel { get; set; }

        /// <summary>
        /// Outline color for redactions in HTML hex.
        /// </summary>
        public string RedactOutline { get; set; }

        /// <summary>
        /// Fill color for redactions in HTML hex.
        /// </summary>
        public string RedactFill { get; set; }
        
        /// <summary>
        /// Search string for redaction beginnings.
        /// </summary>
        public string? RedactStart { get; set; }

        /// <summary>
        /// Search string for redaction endings.
        /// </summary>
        public string? RedactEnd { get; set; }

        /// <summary>
        /// Colors used in modifications in HTML hex -format.
        /// </summary>
        public string[] Colors { get; set; }

        /// <summary>
        /// Suffix appended to filenames when redacting.
        /// </summary>
        public string? RedactFileSuffix { get; set; }

        /// <summary>
        /// If true, apply redactions when redacting.
        /// </summary>
        public bool RedactExecuteApply { get; set; }

        /// <summary>
        /// If true, do not search for redactable text but only apply
        /// the redactions already in the document.
        /// </summary>
        public bool RedactApplyOnly { get; set; }

        /// <summary>
        /// If true, will attempt to redact specified ranges.
        /// </summary>
        public bool RedactRange { get; set; }

        /// <summary>
        /// If true, will attempt to redact specified words.
        /// </summary>
        public bool RedactWords { get; set; }
    }
}
