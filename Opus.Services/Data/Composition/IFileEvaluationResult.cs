using Opus.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Data.Composition
{
    /// <summary>
    /// Whether the search file matched or not.
    /// </summary>
    public enum OutcomeType
    {
        Match,
        NoMatch
    }

    /// <summary>
    /// Result from evaluating a file path against a composition segment rule.
    /// </summary>
    public interface IFileEvaluationResult : ISelectable
    {
        /// <summary>
        /// Outcome of the comparison.
        /// </summary>
        public OutcomeType Outcome { get; }

        /// <summary>
        /// Path of the file to compare.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Name of the result.
        /// </summary>
        public string Name { get; }
    }
}
