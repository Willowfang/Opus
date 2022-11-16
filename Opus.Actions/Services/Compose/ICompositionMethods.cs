using Opus.Actions.Services.Base;
using Opus.Common.Services.Data.Composition;

namespace Opus.Actions.Services.Compose
{
    /// <summary>
    /// Methods for composition action.
    /// </summary>
    public interface ICompositionMethods : IActionMethods
    {
        /// <summary>
        /// Compose a pdf document.
        /// </summary>
        /// <param name="directory">Directory to perform the composition from.</param>
        /// <param name="compositionProfile">Profile the composition is based on.</param>
        /// <returns></returns>
        public Task Compose(
            string directory,
            ICompositionProfile compositionProfile);

        /// <summary>
        /// Execute the actual composition.
        /// </summary>
        /// <param name="directory">The directory to compose.</param>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteComposition(string directory);

        /// <summary>
        /// Execution method for editability.
        /// <para>
        /// Makes the profile (un)editable and saves it.
        /// </para>
        /// </summary>
        public void ExecuteEditable();

        /// <summary>
        /// Execution method for profile editing.
        /// <para>
        /// Open a dialog for editing the profile.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteEditProfile();

        /// <summary>
        /// Execution method for profile adding.
        /// <para>
        /// Opens a dialog for entering info on the new profile.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteAddProfile();

        /// <summary>
        /// Execution method for profile deletion.
        /// <para>
        /// Confirms deletion from the user and deletes the profile, if confirmation was received.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteDeleteProfile();

        /// <summary>
        /// Execution method for profile copying.
        /// <para>
        /// Copies the profile as a new profile.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteCopyProfile();

        /// <summary>
        /// Execution method for import profile.
        /// <para>
        /// Opens the importable profile file, reads it, imports the profile if possible and closes the file.muistio
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteImportProfile();

        /// <summary>
        /// Execution method for profile export.
        /// <para>
        /// Exports the current profile as .opusprofile -file.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteExportProfile();

        /// <summary>
        /// Execution method for file segment addition.
        /// <para>
        /// Adds the file segment into the profile.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public Task ExecuteAddFileSegment();

        /// <summary>
        /// Execution method for profile segment edit.
        /// <para>
        /// Asks for values to edit and modifies the segment accordingly.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public Task ExecuteEditSegment();

        /// <summary>
        /// Execution method for title segment addition.
        /// <para>
        /// Adds a new title segment into the profile.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public Task ExecuteAddTitleSegment();

        /// <summary>
        /// Execution method for segment deletion.
        /// <para>
        /// Delete a segment from a profile.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public Task ExecuteDeleteSegment();
    }
}
