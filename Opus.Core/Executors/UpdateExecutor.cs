using CX.PdfLib.Common;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using Opus.Values;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Opus.Core.Executors
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public string[] Notes { get; set; }
        public string SetupFileDirectory { get; set; }
    }

    public interface IUpdateExecutor
    {
        public Task<bool> InitializeUpdate();
        public bool CheckForUpdates();
    }

    public class UpdateExecutor : IUpdateExecutor
    {
        private IDialogAssist dialogAssist;
        private UpdateInfo info;
        private UpdateInfo remoteInfo;

        public UpdateExecutor(IDialogAssist dialogAssist)
        {
            this.dialogAssist = dialogAssist;
            try
            {
                info = JsonSerializer.Deserialize<UpdateInfo>(File.ReadAllText(FilePaths.LOCALUPDATEINFOLOCATION));
            }
            catch (Exception)
            {
                info = null;
            }
        }

        public bool CheckForUpdates()
        {
            if (info == null)
                return false;

            Version currentVersion = new Version(info.Version);

            string remoteInfoPath = Path.Combine(info.SetupFileDirectory, FilePaths.UPDATEINFONAME);

            if (!File.Exists(remoteInfoPath))
                return false;

            remoteInfo = JsonSerializer.Deserialize<UpdateInfo>(File.ReadAllText(remoteInfoPath));
            Version remoteInfoVersion = new Version(remoteInfo.Version);

            return remoteInfoVersion > currentVersion;
        }

        public async Task<bool> InitializeUpdate()
        {
            if (remoteInfo == null)
                return false;

            string updateFileLocation = Path.Combine(info.SetupFileDirectory, FilePaths.SETUPFILENAME);
            string updateInfoLocation = Path.Combine(info.SetupFileDirectory, FilePaths.UPDATEINFONAME);

            string tempInstallerLocation = Path.Combine(Path.GetTempPath(), Path.GetFileName(updateFileLocation));
            string tempUpdateInfoLocation = Path.Combine(Path.GetTempPath(), FilePaths.UPDATEINFONAME);

            UpdateDialog message = new UpdateDialog(Resources.Labels.General.Update,
                remoteInfo.Version, remoteInfo.Notes);

            await dialogAssist.Show(message);

            if (message.IsCanceled)
                return false;

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            ProgressContainer container = dialogAssist.ShowProgress(tokenSource);

            await Task.Run(() => File.Copy(updateFileLocation, tempInstallerLocation, true));
            await Task.Run(() => File.Copy(updateInfoLocation, tempUpdateInfoLocation, true));

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
