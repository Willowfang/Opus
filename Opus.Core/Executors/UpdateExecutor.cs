using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using Opus.Values;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Opus.Core.Executors
{
    /// <summary>
    /// Information on a pending update.
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// New version number.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Notes about the new update (new features, bug fixes, etc).
        /// </summary>
        public string[] Notes { get; set; }

        /// <summary>
        /// Where the update file for the next update will be stored.
        /// </summary>
        public string SetupFileDirectory { get; set; }
    }

    /// <summary>
    /// Service for checking for and performing software updates.
    /// <para>Default implementation is in the same namespace.</para>
    /// </summary>
    public interface IUpdateExecutor
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

    /// <summary>
    /// Default implementation for <see cref="IUpdateExecutor"/>.
    /// </summary>
    public class UpdateExecutor : IUpdateExecutor
    {
        // DI services and state
        private IDialogAssist dialogAssist;
        private UpdateInfo info;
        private UpdateInfo remoteInfo;

        /// <summary>
        /// Create a new update executor.
        /// </summary>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        public UpdateExecutor(IDialogAssist dialogAssist)
        {
            this.dialogAssist = dialogAssist;

            // Try to fetch update information from local filesystem (for comparison).

            try
            {
                info = JsonSerializer.Deserialize<UpdateInfo>(
                    File.ReadAllText(FilePaths.LOCALUPDATEINFOLOCATION)
                );
            }
            catch (Exception)
            {
                info = null;
            }
        }

        /// <summary>
        /// Check for updates in the given location.
        /// </summary>
        /// <returns>Info on whether an update is available.</returns>
        public bool CheckForUpdates()
        {
            if (info == null)
                return false;

            // Get current and update version numbers and compare them. If update version is newer,
            // there is a new update. Otherwise, not.

            Version currentVersion = new Version(info.Version);

            string remoteInfoPath = Path.Combine(info.SetupFileDirectory, FilePaths.UPDATEINFONAME);

            if (!File.Exists(remoteInfoPath))
                return false;

            remoteInfo = JsonSerializer.Deserialize<UpdateInfo>(File.ReadAllText(remoteInfoPath));
            Version remoteInfoVersion = new Version(remoteInfo.Version);

            return remoteInfoVersion > currentVersion;
        }

        /// <summary>
        /// Start updating this software.
        /// </summary>
        /// <returns>Info on whether the update was successful.</returns>
        public async Task<bool> InitializeUpdate()
        {
            if (remoteInfo == null)
                return false;

            // Get correct paths.

            string updateFileLocation = Path.Combine(
                info.SetupFileDirectory,
                FilePaths.SETUPFILENAME
            );
            string updateInfoLocation = Path.Combine(
                info.SetupFileDirectory,
                FilePaths.UPDATEINFONAME
            );

            string tempInstallerLocation = Path.Combine(
                Path.GetTempPath(),
                Path.GetFileName(updateFileLocation)
            );
            string tempUpdateInfoLocation = Path.Combine(
                Path.GetTempPath(),
                FilePaths.UPDATEINFONAME
            );

            UpdateDialog message = new UpdateDialog(
                Resources.Labels.General.Update,
                remoteInfo.Version,
                remoteInfo.Notes
            );

            await dialogAssist.Show(message);

            if (message.IsCanceled)
                return false;

            // Show progress for copying the update file.

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            ProgressContainer container = dialogAssist.ShowProgress(tokenSource);

            await Task.Run(() => File.Copy(updateFileLocation, tempInstallerLocation, true));
            await Task.Run(() => File.Copy(updateInfoLocation, tempUpdateInfoLocation, true));

            // Start the update as a separate process. Update automatically without showing
            // the options.

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = true;
            processStartInfo.FileName = tempInstallerLocation;
            processStartInfo.Arguments = "/passive UPDATING_AUTOMATICALLY=1";

            Process.Start(processStartInfo);

            container.ProgressDialog.CloseOnError();

            await container.Show;

            if (container.ProgressDialog.IsCanceled)
                return false;

            return true;
        }
    }
}
