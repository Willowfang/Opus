using Opus.Actions.Services.Update;
using Opus.Values;
using System.Text.Json;

namespace Opus.Actions.Implementation.Update
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class UpdateProperties : IUpdateProperties
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public UpdateInfo? LocalVersionInfo { get; set; }
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public UpdateInfo? RemoteVersionInfo {get; set; }

        /// <summary>
        /// Create new update properties instance.
        /// </summary>
        public UpdateProperties()
        {
            // Try to fetch update information from local filesystem (for comparison).

            try
            {
                LocalVersionInfo = JsonSerializer.Deserialize<UpdateInfo>(
                    File.ReadAllText(FilePaths.LOCALUPDATEINFOLOCATION)
                );
            }
            catch (Exception)
            {
                LocalVersionInfo = null;
            }
        }
    }
}
