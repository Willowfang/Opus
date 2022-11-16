namespace Opus.Actions.Services.Update
{
    /// <summary>
    /// Methods for application update
    /// </summary>
    public interface IUpdateMethods
    {
        /// <summary>
        /// Start updating this software.
        /// </summary>
        /// <returns>An awaitable task. The task will return info on whether the
        /// update was successful.</returns>
        public Task<bool> InitializeUpdate();

        /// <summary>
        /// Check if there are new updates in the update path.
        /// </summary>
        /// <returns>Info on whether there are new updates.</returns>
        public bool CheckForUpdates();
    }
}
