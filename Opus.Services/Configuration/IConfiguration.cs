using Opus.Services.Data.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Configuration
{
    /// <summary>
    /// Application configuration service
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Application language in ISO639-1 format
        /// </summary>
        public string LanguageCode { get; set; }
        /// <summary>
        /// Title for extraction
        /// </summary>
        public string ExtractionTitle { get; set; }
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
        /// Annotation handling option
        /// </summary>
        public int Annotations { get; set; }
        /// <summary>
        /// Indicates whether individual bookmarks are grouped by source files
        /// </summary>
        public bool GroupByFiles { get; set; }
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
    }
}
