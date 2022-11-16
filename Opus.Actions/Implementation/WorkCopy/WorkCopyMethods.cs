using Opus.Actions.Services.WorkCopy;
using Opus.Common.Extensions;
using Opus.Common.Logging;
using Opus.Common.Dialogs;
using Opus.Common.Services.Input;
using WF.LoggingLib;
using Opus.Common.Services.Dialogs;
using Opus.Common.Services.Configuration;
using WF.PdfLib.Services;
using Opus.Common.Wrappers;
using WF.PdfLib.Common;
using Opus.Common.Progress;

namespace Opus.Actions.Implementation.WorkCopy
{
    /// <summary>
    /// Implementation for <see cref="IWorkCopyMethods"/>.
    /// </summary>
    public class WorkCopyMethods : LoggingCapable<WorkCopyMethods>, IWorkCopyMethods
    {
        private readonly IWorkCopyProperties properties;
        private readonly IPathSelection pathSelection;
        private readonly IDialogAssist dialogAssist;
        private readonly IConfiguration configuration;
        private readonly IAnnotationService annotationService;
        private readonly ISigningService signingService;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="properties">Work copy properties service.</param>
        /// <param name="pathSelection">User path selection service.</param>
        /// <param name="dialogAssist">Service for showing dialogs.</param>
        /// <param name="configuration">Application configuration service.</param>
        /// <param name="annotationService">Service for manipulating annotations.</param>
        /// <param name="signingService">Signature services.</param>
        /// <param name="logbook">Logging service.</param>
        public WorkCopyMethods(
            IWorkCopyProperties properties,
            IPathSelection pathSelection,
            IDialogAssist dialogAssist,
            IConfiguration configuration,
            IAnnotationService annotationService,
            ISigningService signingService,
            ILogbook logbook) : base(logbook)
        {
            this.properties = properties;
            this.pathSelection = pathSelection;
            this.dialogAssist = dialogAssist;
            this.configuration = configuration;
            this.annotationService = annotationService;
            this.signingService = signingService;
        }

        #region Remove
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task<IList<FileInfo>> Remove(IEnumerable<FileStorage> files, DirectoryInfo destination)
        {
            logbook.Write("Removing signatures.", LogLevel.Debug);

            // Show progress to user.

            int grandTotal = configuration.WorkCopyFlattenRedactions ? files.Count() * 2 : files.Count();

            ProgressTracker progress = new ProgressTracker(grandTotal, dialogAssist);

            // Get naming template for the products.

            string destinationTemplate = RemoveGetTemplate();

            List<FileInfo> createdFiles = new List<FileInfo>();

            // Get all removals as tasks.

            List<Task> removalTasks = RemoveGetRemovalTasks(
                files.ToList(), 
                destinationTemplate, 
                createdFiles, 
                destination,
                progress);

            // Do removals.

            AggregateException? removalException = await RemoveTryAllRemovals(removalTasks);

            // If removals had errors, do clean up and display error message to user.

            if (removalException != null)
            {
                RemoveCleanUp(removalException);

                await RemoveShowErrorMessageDialog();

                progress.Cancel();
            }

            // Flatten redactions, if the user has chosen so.

            if (configuration.WorkCopyFlattenRedactions)
            {
                await RemoveFlattenRedactions(createdFiles, progress);
            }

            progress.Update(100, ProgressPhase.Finished);

            await progress.Show;

            logbook.Write($"Signatures removed.", LogLevel.Information);

            return createdFiles;
        }

        private async Task RemoveFlattenRedactions(IList<FileInfo> files, ProgressTracker tracker)
        {
            logbook.Write($"Flattening redactions.", LogLevel.Debug);

            List<Task> redTasks = new List<Task>();
            foreach (FileInfo file in files)
            {
                redTasks.Add(FlattenTaskWithProgressUpdate(file.FullName, tracker));
            }

            await Task.WhenAll(redTasks);

            logbook.Write($"Redactions flattened.", LogLevel.Debug);
        }

        private async Task FlattenTaskWithProgressUpdate(string input, ProgressTracker tracker)
        {
            await annotationService.FlattenRedactions(input);
            tracker.Update(1);
        }

        private string RemoveGetTemplate()
        {
            // If template has not been defined, assign the default template.

            if (string.IsNullOrEmpty(configuration.UnsignedTitleTemplate))
                configuration.UnsignedTitleTemplate = Resources.DefaultValues.DefaultValues.UnsignedTemplate;

            // Get the name template for products.

            return configuration.UnsignedTitleTemplate + Values.FilePaths.PDF_EXTENSION;
        }

        private List<Task> RemoveGetRemovalTasks(
            IList<FileStorage> files,
            string destinationTemplate,
            List<FileInfo> createdFiles,
            DirectoryInfo destination,
            ProgressTracker tracker)
        {
            logbook.Write($"Retrieving removal tasks.", LogLevel.Debug);

            List<Task> removalTasks = new List<Task>();

            // Create a new task for each file

            for (int i = 0; i < files.Count; i++)
            {
                string destinationName = destinationTemplate
                    .ReplacePlaceholder(
                        Placeholders.File,
                        Path.GetFileNameWithoutExtension(files[i].FilePath))
                    .ReplacePlaceholder(
                        Placeholders.Number,
                        (i + 1).ToString());

                FileInfo finalDestination = new FileInfo(
                    Path.Combine(destination.FullName, destinationName)
                );

                createdFiles.Add(finalDestination);

                removalTasks.Add(RemovalTaskWithProgressUpdate(
                    new FileInfo(files[i].FilePath), 
                    finalDestination, 
                    tracker));
            }

            logbook.Write($"Removal tasks retrieved.", LogLevel.Debug);

            return removalTasks;
        }

        private async Task RemovalTaskWithProgressUpdate(
            FileInfo source, 
            FileInfo destination, 
            ProgressTracker tracker)
        {
            await signingService.RemoveSignature(source, destination, tracker.Token);
            tracker.Update(1);
        }

        private async Task<AggregateException?> RemoveTryAllRemovals(List<Task> removalTasks)
        {
            logbook.Write($"Attempt all removals.", LogLevel.Debug);

            Task? allRemovals = null;

            try
            {
                allRemovals = Task.WhenAll(removalTasks);
                await allRemovals;
            }
            catch (Exception e)
            {
                logbook.Write($"Removals failed.", LogLevel.Error, e);
            }

            return allRemovals?.Exception;
        }

        private void RemoveCleanUp(AggregateException exception)
        {
            foreach (Exception inner in exception.InnerExceptions)
            {
                File.Delete(inner.Message);
            }
        }

        private async Task RemoveShowErrorMessageDialog()
        {
            MessageDialog message = new MessageDialog(
                Resources.Labels.General.Error,
                Resources.Messages.Signature.RemovalFailed);

            await dialogAssist.Show(message);
        }

        #endregion

        #region Execute delete
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteDelete()
        {
            logbook.Write($"Deleting files.", LogLevel.Information);

            properties.OriginalFiles.RemoveAll(x => x.IsSelected);

            logbook.Write($"Files deleted.", LogLevel.Information);
        }
        #endregion

        #region Execute clear
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteClear()
        {
            logbook.Write($"Clearing files.", LogLevel.Information);

            properties.OriginalFiles.Clear();

            logbook.Write($"Files cleared.", LogLevel.Information);
        }
        #endregion

        #region Execute create work copy
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task ExecuteCreateWorkCopy()
        {
            logbook.Write($"Creating work copies.", LogLevel.Information);

            // Empty check.

            if (properties.OriginalFiles.Count < 1) return;

            // Ask for save path.

            string? path = pathSelection.OpenDirectory(Resources.UserInput.Descriptions.SelectSaveFolder);

            if (string.IsNullOrEmpty(path))
            {
                logbook.Write($"Selected path is null or empty.", LogLevel.Warning);

                return;
            }

            await Remove(properties.OriginalFiles, new DirectoryInfo(path));

            logbook.Write($"Work copies created.", LogLevel.Information);
        }
        #endregion
    }
}
