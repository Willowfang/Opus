using Opus.Actions.Services.Merge;
using Opus.Common.Wrappers;
using Opus.Common.Logging;
using Opus.Common.Dialogs;
using Opus.Common.Services.Dialogs;
using WF.LoggingLib;
using WF.PdfLib.Common;
using WF.PdfLib.Services.Data;
using Opus.Common.Services.Input;
using WF.PdfLib.Services;
using Opus.Common.Services.Configuration;
using Opus.Common.Progress;

namespace Opus.Actions.Implementation.Merge
{
    /// <summary>
    /// Implementation for <see cref="IMergeMethods"/>.
    /// </summary>
    public class MergeMethods : LoggingCapable<MergeMethods>, IMergeMethods
    {
        private readonly IDialogAssist dialogAssist;
        private readonly IMergeProperties properties;
        private readonly IPathSelection pathSelection;
        private readonly IMergingService mergingService;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="dialogAssist">Service for showing dialogs.</param>
        /// <param name="properties">Merge properties service.</param>
        /// <param name="pathSelection">User path selection service.</param>
        /// <param name="mergingService">Service for merging files together.</param>
        /// <param name="configuration">Application configuration service.</param>
        /// <param name="logbook">Logging service.</param>
        public MergeMethods(
            IDialogAssist dialogAssist,
            IMergeProperties properties,
            IPathSelection pathSelection,
            IMergingService mergingService,
            IConfiguration configuration,
            ILogbook logbook) : base(logbook)
        {
            this.dialogAssist = dialogAssist;
            this.properties = properties;
            this.pathSelection = pathSelection;
            this.mergingService = mergingService;
            this.configuration = configuration;
        }

        #region Get merge inputs
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public IList<IMergeInput> GetMergeInputs(IMergeProperties properties)
        {
            logbook.Write($"Retrieving merge inputs.", LogLevel.Debug);

            List<IMergeInput> inputs = new List<IMergeInput>();

            foreach (FileStorage file in properties.Collection)
            {
                inputs.Add(new MergeInput(file.FilePath, file.Title, file.Level));
            }

            logbook.Write($"Merge inputs retrieved.", LogLevel.Debug);

            return inputs;
        }
        #endregion

        #region Execute edit
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task ExecuteEdit()
        {
            logbook.Write($"Editing file.", LogLevel.Information);

            // Create and show a dialog for editing the entry.

            FileTitleDialog? dialog = await ShowEditDialog(properties);

            // If cancelled, just return.

            if (dialog == null || dialog.IsCanceled)
            {
                logbook.Write($"Edit was cancelled or selected file was null.", LogLevel.Information);

                return;
            }

            if (dialog.Title == null || properties.Collection.SelectedItem == null)
            {
                logbook.Write($"Selected item or given title was null.", LogLevel.Warning);

                return;
            }

            // Change the title of the selected item according to user preferences.

            properties.Collection.SelectedItem.Title = dialog.Title;

            logbook.Write($"File edited.", LogLevel.Information);
        }

        private async Task<FileTitleDialog?> ShowEditDialog(IMergeProperties properties)
        {
            if (properties.Collection.SelectedItem == null) return null;

            FileTitleDialog dialog = new FileTitleDialog(
                Resources.Labels.Dialogs.FileTitle.Edit)
            {
                Title = properties.Collection.SelectedItem.Title
            };

            await dialogAssist.Show(dialog);

            return dialog;
        }
        #endregion

        #region Execute delete
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteDelete()
        {
            logbook.Write($"Deleting file.", LogLevel.Information);

            properties.Collection.RemoveSelected();

            logbook.Write($"File deleted.", LogLevel.Information);
        }
        #endregion

        #region Execute clear
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteClear()
        {
            logbook.Write($"Clearing files.", LogLevel.Information);

            properties.Collection.Clear();

            logbook.Write($"Files cleared.", LogLevel.Information);
        }
        #endregion

        #region Execute merge
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task ExecuteMerge()
        {
            logbook.Write($"Merging files.", LogLevel.Information);

            string? path = pathSelection.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.PDF
            );

            if (string.IsNullOrEmpty(path))
            {
                logbook.Write($"Selected file path was null or empty.", LogLevel.Warning);

                return;
            }

            IList<IMergeInput> inputs = await Task.Run(() => GetMergeInputs(properties));

            ProgressTracker progress = new ProgressTracker(100, dialogAssist);

            MergingOptions options = MergeGetOptions(inputs, path, progress);

            logbook.Write($"Attempting merge.", LogLevel.Debug);

            try
            {
                await mergingService.MergeWithOptions(options);
            }
            catch (Exception e)
            {
                logbook.Write($"Merge failed.", LogLevel.Error, e);

                await MergeShowMergeFailedMessage(progress, e);
            }

            await progress.Show;

            logbook.Write($"Merging completed.", LogLevel.Information);
        }

        private MergingOptions MergeGetOptions(
            IList<IMergeInput> inputs,
            string savePath,
            ProgressTracker tracker)
        {
            logbook.Write($"Retrieving merge options.", LogLevel.Debug);

            MergingOptions options = new MergingOptions(
                inputs,
                new FileInfo(savePath),
                configuration.MergeAddPageNumbers,
                false
            );

            options.Cancellation = tracker.Token;

            options.Progress = tracker.ProgressInterface;

            logbook.Write($"Merge options retrieved.", LogLevel.Debug);

            return options;
        }

        private async Task MergeShowMergeFailedMessage(ProgressTracker tracker, Exception e)
        {
            tracker.Cancel();

            MessageDialog message = new MessageDialog(
                Resources.Labels.General.Error,
                Resources.Messages.Merging.MergeFailed
            );

            await dialogAssist.Show(message);
        }
        #endregion
    }
}
