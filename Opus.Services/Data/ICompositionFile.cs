using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Opus.Services.Data
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
        public Regex SearchTerm { get; }
        /// <summary>
        /// Set the search term. Creates a compiled regex for faster searching.
        /// </summary>
        /// <param name="regex">Regex string</param>
        public void SetSearchTerm(string regex);
        /// <summary>
        /// Parts of the filename to exclude from search and final file name
        /// </summary>
        public Regex ToRemove { get; }
        /// <summary>
        /// Set the part to ignore and remove. Creates a compiled regex for faster removal.
        /// </summary>
        /// <param name="regex"></param>
        public void SetToRemove(string regex);
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
        /// Evaluate a file for a match against <see cref="SearchTerm"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public IFileEvaluationResult EvaluateFile(string filePath);
    }
}
