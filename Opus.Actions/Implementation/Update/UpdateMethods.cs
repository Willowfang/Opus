using Opus.Actions.Services.Update;
using Opus.Common.Logging;
using Opus.Common.Dialogs;
using Opus.Common.Services.Dialogs;
using Opus.Values;
using System.Diagnostics;
using System.Text.Json;
using WF.LoggingLib;
using Opus.Common.Progress;

namespace Opus.Actions.Implementation.Update
{
    /// <summary>
    /// Class for storing file paths related to updates.
    /// </summary>
    public class UpdateFileLocations
    {
        /// <summary>
        /// Location for the setup file.
        /// </summary>
        public string SetupFile { get; }

        /// <summary>
        /// Location for info on setup.
        /// </summary>
        public string SetupInfo { get; }

        /// <summary>
        /// Location for temporary setup file storage.
        /// </summary>
        public string TempSetupFile { get; }

        /// <summary>
        /// Location for temporary setup info storage.
        /// </summary>
        public string TempInfo { get; }

        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="setupFile">Location for setup file.</param>
        /// <param name="setupInfo">Location for setup info.</param>
        /// <param name="tempSetupFile">Location for temporary setup file.</param>
        /// <param name="tempInfo">Location for temporary info.</param>
        public UpdateFileLocations(
            string setupFile, 
            string setupInfo, 
            string tempSetupFile, 
            string tempInfo)
        {
            SetupFile = setupFile;
            SetupInfo = setupInfo;
            TempSetupFile = tempSetupFile;
            TempInfo = tempInfo;
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class UpdateMethods : LoggingCapable<UpdateMethods>, IUpdateMethods
    {
        private readonly IDialogAssist dialogAssist;
        private readonly IUpdateProperties properties;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="dialogAssist">Service for showing dialogs.</param>
        /// <param name="properties">Update properties service.</param>
        /// <param name="logbook">Logging service.</param>
        public UpdateMethods(
            IDialogAssist dialogAssist,
            IUpdateProperties properties,
            ILogbook logbook) : base(logbook)
        {
            this.dialogAssist = dialogAssist;
            this.properties = properties;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CheckForUpdates()
        {
            // Null checks.

            if (properties.LocalVersionInfo == null
                || properties.LocalVersionInfo.Version == null
                || properties.LocalVersionInfo.SetupFileDirectory == null)
            {
                logbook.Write("A null argument was provided for update check.", LogLevel.Warning);
                return false;
            }

            logbook.Write("Checking for updates.", LogLevel.Information);

            // Get current and update version numbers and compare them. If update version is newer,
            // there is a new update. Otherwise, not.

            Version currentVersion = new Version(properties.LocalVersionInfo.Version);

            string remoteInfoPath = Path.Combine(properties.LocalVersionInfo.SetupFileDirectory, FilePaths.UPDATEINFONAME);

            if (!File.Exists(remoteInfoPath))
                return false;

            properties.RemoteVersionInfo = JsonSerializer.Deserialize<UpdateInfo>(File.ReadAllText(remoteInfoPath));

            if (properties.RemoteVersionInfo == null
                || properties.RemoteVersionInfo.Version == null)
                return false;

            Version remoteInfoVersion = new Version(properties.RemoteVersionInfo.Version);

            logbook.Write("Update checking complete.", LogLevel.Information);

            return remoteInfoVersion > currentVersion;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task<bool> InitializeUpdate()
        {
            if (properties.RemoteVersionInfo == null)
            {
                logbook.Write("Remote version info was null.", LogLevel.Warning);
                return false;
            }

            logbook.Write("Application update initialized.", LogLevel.Information);

            // Get correct paths.

            UpdateFileLocations? locations = GetFileLocationsForUpdate();

            if (locations == null)
            {
                logbook.Write($"File locations were null.", LogLevel.Warning);

                return false;
            }

            UpdateDialog dialog = await ShowUpdateDialog(properties.RemoteVersionInfo);

            if (dialog.IsCanceled)
            {
                logbook.Write($"Update was cancelled.", LogLevel.Information);

                return false;
            }

            // Show progress for copying the update file.

            ProgressTracker tracker = new ProgressTracker(100, dialogAssist);

            await CopyFilesToLocalDirectory(locations);

            tracker.Cancel();

            // Start the update as a separate process. Update automatically without showing
            // the options.

            ExecuteUpdateSeparateProcess(locations);

            logbook.Write($"Update initialized externally. Good bye and see you soon!", LogLevel.Information);

            return true;
        }

        private void ExecuteUpdateSeparateProcess(UpdateFileLocations locations)
        {
            logbook.Write("Calling update file execution in a separate process.", LogLevel.Debug);

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.UseShellExecute = true;
                processStartInfo.FileName = locations.TempSetupFile;
                processStartInfo.Arguments = "/passive UPDATING_AUTOMATICALLY=1";

                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                logbook.Write("Update file execution process caused an error.", LogLevel.Error, ex);
                throw;
            }

            logbook.Write($"External process started.", LogLevel.Debug);
        }

        private async Task CopyFilesToLocalDirectory(UpdateFileLocations locations)
        {
            logbook.Write("Copying update files to local directory.", LogLevel.Debug);

            try
            {
                await Task.Run(() => File.Copy(locations.SetupFile, locations.TempSetupFile, true));
                await Task.Run(() => File.Copy(locations.SetupInfo, locations.TempInfo, true));
            }
            catch (Exception ex)
            {
                logbook.Write("Update file copy caused an error.", LogLevel.Error, ex);
                throw;
            }

            logbook.Write($"File copy completed.", LogLevel.Debug);
        }

        private async Task<UpdateDialog> ShowUpdateDialog(UpdateInfo remoteInfo)
        {
            UpdateDialog dialog = new UpdateDialog(
                Resources.Labels.General.Update,
                remoteInfo.Version ?? "",
                remoteInfo.Notes ?? Array.Empty<string>()
            );

            await dialogAssist.Show(dialog);

            return dialog;
        }

        private UpdateFileLocations? GetFileLocationsForUpdate()
        {
            if (properties.RemoteVersionInfo == null
                || properties.RemoteVersionInfo.Version == null
                || properties.LocalVersionInfo == null
                || properties.LocalVersionInfo.SetupFileDirectory == null)
            {
                logbook.Write("A null argument was provided.", LogLevel.Warning);

                return null;
            }

            logbook.Write("Retrieving file locations for update.", LogLevel.Debug);

            string updateFileLocation = Path.Combine(
                properties.LocalVersionInfo.SetupFileDirectory,
                FilePaths.SETUPFILENAME
            );
            string updateInfoLocation = Path.Combine(
                properties.LocalVersionInfo.SetupFileDirectory,
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

            logbook.Write("Update file locations retrieved.", LogLevel.Debug);

            return new UpdateFileLocations(
                updateFileLocation,
                updateInfoLocation,
                tempInstallerLocation,
                tempUpdateInfoLocation);
        }
    }
}
