using Opus.Services.Data.Composition;
using Opus.Services.Implementation.Base;

namespace Opus.Services.Implementation.Data.Composition
{
    /// <summary>
    /// The result of evaluation of a file against a segment.
    /// </summary>
    public class EvaluationResult : SelectableBase, IFileEvaluationResult
    {
        /// <summary>
        /// Does the file match.
        /// </summary>
        public OutcomeType Outcome { get; }

        /// <summary>
        /// Path of the file evaluated.
        /// </summary>
        public string? FilePath { get; }

        /// <summary>
        /// Name of the segment.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Create a new evaluation result.
        /// </summary>
        /// <param name="outcome">Result outcome - match or not.</param>
        /// <param name="filePath">Path of the file evaluated.</param>
        /// <param name="name">Name of the segment.</param>
        private EvaluationResult(OutcomeType outcome, string? filePath, string? name)
        {
            Outcome = outcome;
            FilePath = filePath;
            Name = name;
        }

        /// <summary>
        /// Create a matching evaluation result.
        /// </summary>
        /// <param name="filePath">Path of the file evaluated.</param>
        /// <param name="name">Name of the segment result.</param>
        /// <returns>Created result.</returns>
        public static IFileEvaluationResult Match(string filePath, string name)
        {
            return new EvaluationResult(OutcomeType.Match, filePath, name);
        }

        /// <summary>
        /// Create a non-matching evaluation result.
        /// </summary>
        /// <returns>Created result.</returns>
        public static IFileEvaluationResult NoMatch()
        {
            return new EvaluationResult(OutcomeType.NoMatch, null, null);
        }
    }
}
