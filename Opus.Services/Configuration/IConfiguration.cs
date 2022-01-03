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
        /// If true, page numbers will be added to a merged document
        /// </summary>
        public bool MergeAddPageNumbers { get; set; }
        /// <summary>
        /// Include subdirectories when searching for composition files
        /// </summary>
        public bool CompositionSearchSubDirectories { get; set; }
        /// <summary>
        /// The <see cref="ICompositionProfile"/> to select by default
        /// </summary>
        public Guid DefaultProfile { get; set; }
    }
}
