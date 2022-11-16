using Opus.Common.Services.Base;

namespace Opus.Common.Services.Data.Composition
{
    /// <summary>
    /// Whether the search file matched or not.
    /// </summary>
    public enum OutcomeType
    {
        /// <summary>
        /// Evaluation resulted in a match.
        /// </summary>
        Match,
        /// <summary>
        /// There was no match for this evaluation.
        /// </summary>
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
        public string? FilePath { get; }

        /// <summary>
        /// Name of the result.
        /// </summary>
        public string? Name { get; }
    }
}
