using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Opus.Services.Data.Composition
{
    public interface ICompositionFile : ICompositionSegment
    {
        /// <summary>
        /// Get name for segment from the filename
        /// </summary>
        public bool NameFromFile { get; set; }
        /// <summary>
        /// The term to search files with
        /// </summary>
        public Regex SearchExpression { get; }
        /// <summary>
        /// Set the search term. Creates a compiled regex for faster searching.
        /// </summary>
        public string SearchExpressionString { get; set; }
        /// <summary>
        /// Parts of the filename to exclude from search and final file name
        /// </summary>
        public Regex IgnoreExpression { get; }
        /// <summary>
        /// Set the part to ignore and remove. Creates a compiled regex for faster removal.
        /// </summary>
        public string IgnoreExpressionString { get; set; }
        /// <summary>
        /// Minimum required number of documents fulfilling the conditions.
        /// 0 indicates no documents matching the conditions need to be found.
        /// </summary>
        public int MinCount { get; set; }
        /// <summary>
        /// Maximun number of documents fulfilling the conditions.
        /// 0 indicates an unlimited number of documents.
        /// </summary>
        public int MaxCount { get; set; }
        /// <summary>
        /// Example file name for displaying to user
        /// </summary>
        public string Example { get; set; }

        /// <summary>
        /// Evaluate a file for a match against <see cref="SearchExpression"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public IFileEvaluationResult EvaluateFile(string filePath);
    }
}
