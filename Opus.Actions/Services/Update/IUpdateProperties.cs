namespace Opus.Actions.Services.Update
{
    /// <summary>
    /// Information on a pending update.
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// New version number.
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Notes about the new update (new features, bug fixes, etc).
        /// </summary>
        public string[]? Notes { get; set; }

        /// <summary>
        /// Where the update file for the next update will be stored.
        /// </summary>
        public string? SetupFileDirectory { get; set; }
    }

    /// <summary>
    /// Properties for application update.
    /// </summary>
    public interface IUpdateProperties
    {
        /// <summary>
        /// Version info for locally installed application.
        /// </summary>
        public UpdateInfo? LocalVersionInfo { get; set; }

        /// <summary>
        /// Version info for installation file at remote location.
        /// </summary>
        public UpdateInfo? RemoteVersionInfo { get; set; }
    }
}
