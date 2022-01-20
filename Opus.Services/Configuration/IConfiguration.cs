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
        /// Prefix for extracted files
        /// </summary>
        public string ExtractionPrefix { get; set; }
        /// <summary>
        /// Suffix for extracted files
        /// </summary>
        public string ExtractionSuffix { get; set; }
        /// <summary>
        /// If true, prefix and suffix values will be asked every time
        /// an extraction is done
        /// </summary>
        public bool ExtractionPrefixSuffixAsk { get; set; }
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
