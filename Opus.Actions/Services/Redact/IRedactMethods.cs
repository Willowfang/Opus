using Opus.Actions.Services.Base;

namespace Opus.Actions.Services.Redact
{
    /// <summary>
    /// Methods for redaction action.
    /// </summary>
    public interface IRedactMethods : IActionMethods
    {
        /// <summary>
        /// Delete a file from the list.
        /// </summary>
        public void ExecuteDelete();

        /// <summary>
        /// Execute redactions with given options.
        /// </summary>
        public Task ExecuteRedactions();

        /// <summary>
        /// Apply redactions found in current documents.
        /// </summary>
        public Task ApplyRedactions();
    }
}
