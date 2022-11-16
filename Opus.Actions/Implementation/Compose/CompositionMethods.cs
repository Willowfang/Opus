using Opus.Actions.Services.Compose;
using Opus.Common.Services.Configuration;
using Opus.Common.Services.Data.Composition;
using Opus.Common.Logging;
using Opus.Common.Dialogs;
using WF.LoggingLib;
using Opus.Common.Services.Dialogs;
using Opus.Common.Services.Input;
using WF.PdfLib.Common;
using WF.PdfLib.Services.Data;
using WF.PdfLib.Services;
using Opus.Common.Progress;
using Opus.Common.Collections;

namespace Opus.Actions.Implementation.Compose
{
    /// <summary>
    /// Implementation for <see cref="ICompositionMethods"/>.
    /// </summary>
    public class CompositionMethods : LoggingCapable<CompositionMethods>, ICompositionMethods
    {
        private readonly IConfiguration configuration;
        private readonly ICompositionProperties properties;
        private readonly ICompositionOptions options;
        private readonly IDialogAssist dialogAssist;
        private readonly IPathSelection pathSelection;
        private readonly IMergingService mergingService;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="configuration">Application configuration service.</param>
        /// <param name="properties">Composition properties service.</param>
        /// <param name="options">Composition options service.</param>
        /// <param name="dialogAssist">Service for showing dialogs.</param>
        /// <param name="pathSelection">Service for the user to select a path.</param>
        /// <param name="mergingService">Service for merging pdf files.</param>
        /// <param name="logbook">Logging service.</param>
        public CompositionMethods(
            IConfiguration configuration,
            ICompositionProperties properties,
            ICompositionOptions options,
            IDialogAssist dialogAssist,
            IPathSelection pathSelection,
            IMergingService mergingService,
            ILogbook logbook) : base(logbook)
        {
            this.configuration = configuration;
            this.properties = properties;
            this.options = options;
            this.dialogAssist = dialogAssist;
            this.pathSelection = pathSelection;
            this.mergingService = mergingService;
        }

        #region Compose
        /// <summary>
        /// Compose files contained in a directory (and possibly subdirectories) into a new
        /// document according to rules laid out in given profile.
        /// </summary>
        /// <param name="directory">Directory to search files in.</param>
        /// <param name="compositionProfile">Profile to compose the document by.</param>
        /// <returns>An awaitable task.</returns>
        public async Task Compose(
            string directory,
            ICompositionProfile compositionProfile)
        {
            logbook.Write($"Starting composition.", LogLevel.Information);

            if (await ComposeValidateArguments(directory, compositionProfile) == false) return;

            IEnumerable<string> files = ComposeGetAllValidFiles(directory, configuration.CompositionSearchSubDirectories);

            int grandTotal = ComposeGetGrandTotal(files, compositionProfile);

            ProgressTracker progress = new ProgressTracker(grandTotal, dialogAssist);

            List<IMergeInput>? inputs = await CompositionGetInputs(
                compositionProfile, 
                files, 
                progress);

            if (inputs == null)
            {
                logbook.Write($"Composition inputs were null. Cancelling action.", LogLevel.Debug);

                progress.Cancel();
                return;
            }

            ComposeRemoveEmptyTitles(inputs);

            if (progress.Token.IsCancellationRequested)
            {
                logbook.Write($"Composition was cancelled.", LogLevel.Debug);

                return;
            }

            bool mergeSuccess = await ComposeMergeInputs(
                directory,
                inputs,
                compositionProfile.AddPageNumbers,
                progress);

            if (mergeSuccess == false)
            {
                await ComposeShowMergeFailedMessageDialog();
                progress.Cancel();
                return;
            }

            logbook.Write($"Composition completed.", LogLevel.Information);
        }

        private async Task ComposeShowMergeFailedMessageDialog()
        {
            MessageDialog message = new MessageDialog(
                Resources.Labels.General.Error,
                Resources.Messages.Merging.MergeFailed);

            await dialogAssist.Show(message);
        }

        private async Task<bool> ComposeMergeInputs(
            string directory,
            List<IMergeInput> inputs,
            bool addPageNumbers,
            ProgressTracker progress)
        {
            logbook.Write($"Starting composition inputs merge.", LogLevel.Debug);

            progress.Update(0, ProgressPhase.ChoosingDestination);
            progress.SetPercentage(0);

            string? filePath =  await Task.Run(() => pathSelection.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.PDF,
                new DirectoryInfo(directory).Name + ".pdf"));

            if (string.IsNullOrEmpty(filePath))
            {
                logbook.Write($"Given path was null. Returning false.", LogLevel.Debug);

                return false;
            }

            progress.Update(0, ProgressPhase.Merging);

            MergingOptions options = ComposeGetMergeOptions(filePath, inputs, addPageNumbers, progress);

            IList<FileSystemInfo>? createdFiles = await ComposeTryMerge(options);

            if (createdFiles == null)
            {
                logbook.Write($"Compose merging failed. Returning false.", LogLevel.Debug);

                return false;
            }

            ComposeMergeCheckDelete(createdFiles, filePath, progress);

            logbook.Write($"Compose inputs merge completed.", LogLevel.Debug);

            return true;
        }

        private void ComposeMergeCheckDelete(
            IList<FileSystemInfo> created,
            string filePath,
            ProgressTracker progress)
        {
            logbook.Write($"Checking for deletion request after merge.", LogLevel.Debug);

            if (progress.Token.IsCancellationRequested == false)
            {
                FileSystemInfo? info = created.FirstOrDefault(x => x.FullName == filePath);
                if (info != null)
                    created.Remove(info);
            }

            if (configuration.CompositionDeleteConverted || progress.Token.IsCancellationRequested)
            {
                logbook.Write($"Deleting created files and folders.", LogLevel.Debug);

                foreach (FileSystemInfo file in created)
                {
                    if (file.Exists)
                        file.Delete();
                }
            }
        }

        private async Task<IList<FileSystemInfo>?> ComposeTryMerge(MergingOptions options)
        {
            logbook.Write($"Attempting merge.", LogLevel.Debug);

            IList<FileSystemInfo>? created;
            try
            {
                created = await mergingService.MergeWithOptions(options);
            }
            catch (Exception e)
            {
                logbook.Write($"Merging failed with error.", LogLevel.Error, e);

                return null;
            }

            return created;
        }

        private MergingOptions ComposeGetMergeOptions(
            string filePath, 
            List<IMergeInput> inputs,
            bool addPageNumbers,
            ProgressTracker progress)
        {
            logbook.Write($"Retrieving merge options.", LogLevel.Debug);

            FileInfo output = new FileInfo(filePath);

            bool convertWord = inputs
                .Where(f => string.IsNullOrEmpty(f.FilePath) == false)
                .Any(i =>
                {
                    string? ext = Path.GetExtension(i.FilePath);

                    if (string.IsNullOrEmpty(ext)) return false;

                    return ext.ToLower().Contains(".doc");
                });

            MergingOptions options = new MergingOptions(
                inputs,
                output,
                addPageNumbers,
                convertWord
            );

            options.Cancellation = progress.Token;

            options.Progress = progress.ProgressInterface;

            logbook.Write($"Merging options retrieved.", LogLevel.Debug);

            return options;
        }

        private void ComposeRemoveEmptyTitles(List<IMergeInput> inputs)
        {
            logbook.Write($"Removing empty titles from merge inputs.", LogLevel.Debug);

            for (int i = inputs.Count - 1; i >= 0; i--)
            {
                IMergeInput current = inputs[i];

                if (current.FilePath != null)
                {
                    continue;
                }

                if (i == inputs.Count - 1)
                {
                    inputs.RemoveAt(i);
                    continue;
                }

                if (inputs[i + 1].Level <= current.Level)
                {
                    inputs.RemoveAt(i);
                    continue;
                }
            }

            logbook.Write($"Empty titles removed.", LogLevel.Debug);
        }

        private async Task<List<IMergeInput>?> CompositionGetInputs(
            ICompositionProfile profile,
            IEnumerable<string> files,
            ProgressTracker progress)
        {
            logbook.Write($"Retrieving composition inputs.", LogLevel.Debug);

            if (profile.Segments == null)
            {
                logbook.Write($"Input retrieval failed. Profile has no segments.", LogLevel.Warning);

                return null;
            }

            List<IMergeInput> inputs = new List<IMergeInput>();

            foreach (ICompositionSegment segment in profile.Segments)
            {
                List<IMergeInput>? segmentInput = await ComposeEvaluateSegment(segment, files, progress);

                if (progress.Token.IsCancellationRequested || segmentInput == null)
                    return null;

                inputs.AddRange(segmentInput);
            }

            logbook.Write($"Composition inputs retrieved.", LogLevel.Debug);

            return inputs;
        }

        private async Task<List<IMergeInput>?> ComposeEvaluateSegment(
            ICompositionSegment segment,
            IEnumerable<string> files,
            ProgressTracker progress)
        {
            logbook.Write($"Evaluating segment.", LogLevel.Debug);

            if (segment is ICompositionFile fileSegment)
            {
                logbook.Write($"Segment is a file segment.", LogLevel.Debug);

                return await ComposeEvaluateFileSegment(fileSegment, files, progress);
            }
            else if (segment is ICompositionTitle titleSegment)
            {
                logbook.Write($"Segment is a title segment.", LogLevel.Debug);

                progress.Update(1, ProgressPhase.Unassigned);

                if (titleSegment.SegmentName == null)
                {
                    logbook.Write($"Title segment addition failed. Name was null.", LogLevel.Warning);

                    return new List<IMergeInput>();
                }

                logbook.Write($"Title segment added.", LogLevel.Debug);

                return new List<IMergeInput>()
                {
                    new MergeInput(null, titleSegment.SegmentName, titleSegment.Level)
                };
            }
            else
            {
                return new List<IMergeInput>();
            }
        }

        private async Task<List<IMergeInput>?> ComposeEvaluateFileSegment(
            ICompositionFile fileSegment,
            IEnumerable<string> files,
            ProgressTracker progress)
        {
            // Get results for each file and store results in a list.

            List<IFileEvaluationResult> evaluationResults = new List<IFileEvaluationResult>();
            foreach (string file in files)
            {
                IFileEvaluationResult result = fileSegment.EvaluateFile(file);
                if (result.Outcome == OutcomeType.Match)
                {
                    logbook.Write($"Evaluation match found with file: {file}.", LogLevel.Debug);

                    evaluationResults.Add(result);
                }

                progress.Update(1, ProgressPhase.Unassigned);
            }

            // Check for the correct amounts of matching files and
            // ask to choose correct files if appropriate.

            return await ComposeCheckForFileCounts(evaluationResults, fileSegment);
        }

        private async Task<List<IMergeInput>?> ComposeCheckForFileCounts(
            List<IFileEvaluationResult> evaluationResults,
            ICompositionFile fileSegment)
        {
            logbook.Write($"Checking for file count matches.", LogLevel.Debug);

            if (fileSegment.MaxCount == 0)
            {
                if (evaluationResults.Count >= fileSegment.MinCount)
                {
                    logbook.Write($"File count withing allowed limits. Adding for merge.", LogLevel.Debug);

                    return ComposeAddFileMerges(evaluationResults, fileSegment);
                }
                else
                {
                    logbook.Write($"File count mismatch. Asking for correct files.", LogLevel.Debug);

                    return await ComposeAskForCorrectFiles(evaluationResults, fileSegment);
                }
            }
            else
            {
                if (evaluationResults.Count >= fileSegment.MinCount
                    && evaluationResults.Count <= fileSegment.MaxCount)
                {
                    logbook.Write($"File count withing allowed limits. Adding for merge.", LogLevel.Debug);

                    return ComposeAddFileMerges(evaluationResults, fileSegment);
                }
                else
                {
                    logbook.Write($"File count mismatch. Asking for correct files.", LogLevel.Debug);

                    return await ComposeAskForCorrectFiles(evaluationResults, fileSegment);
                }
            }
        }

        private List<IMergeInput>? ComposeAddFileMerges(
            IList<IFileEvaluationResult> results,
            ICompositionFile fileSegment)
        {
            List<IMergeInput>? mergeInputs = new List<IMergeInput>();
            foreach (IFileEvaluationResult result in results)
            {
                string? name;

                if (fileSegment.NameFromFile)
                {
                    if (result.Name == null)
                    {
                        mergeInputs = null;
                        break;
                    }

                    name = result.Name;
                }
                else
                {
                    if (fileSegment.SegmentName == null)
                    {
                        mergeInputs = null;
                        break;
                    }

                    name = fileSegment.SegmentName;
                }

                mergeInputs.Add(new MergeInput(result.FilePath, name, fileSegment.Level));
            }

            if (mergeInputs == null)
            {
                logbook.Write($"Segment merge add failed. One or more results do not have a name.",
                    LogLevel.Warning);
            }

            return mergeInputs;
        }

        private async Task<List<IMergeInput>?> ComposeAskForCorrectFiles(
            IList<IFileEvaluationResult> results,
            ICompositionFile fileSegment)
        {
            CompositionFileCountDialog countDialog = new CompositionFileCountDialog(
                results,
                fileSegment,
                pathSelection,
                Resources.Labels.Dialogs.CompositionFileCount.SearchResult
            );

            await dialogAssist.Show(countDialog);

            if (countDialog.IsCanceled)
            {
                return null;
            }
            return ComposeAddFileMerges(countDialog.Results, fileSegment);
        }

        private int ComposeGetGrandTotal(IEnumerable<string> files, ICompositionProfile profile)
        {
            logbook.Write($"Counting grand total for progress.", LogLevel.Debug);

            int totalAmount = 100;

            if (profile.Segments == null) return totalAmount;

            foreach (ICompositionSegment cs in profile.Segments)
            {
                if (cs is ICompositionFile)
                {
                    totalAmount += files.Count();
                }
                else if (cs is ICompositionTitle)
                {
                    totalAmount++;
                }
            }

            logbook.Write($"Grand total counted.", LogLevel.Debug);

            return totalAmount;
        }

        private async Task<bool> ComposeValidateArguments(string directory, ICompositionProfile profile)
        {
            logbook.Write($"Validating composition arguments.", LogLevel.Debug);

            Exception? exception = null;

            if (directory == null)
            {
                await ComposeShowErrorMessage(Resources.Messages.Composition.PathNull);
                exception = new ArgumentNullException(nameof(directory));
            }
            else if (Directory.Exists(directory) == false)
            {
                await ComposeShowErrorMessage(Resources.Messages.Composition.PathNotExists);
                exception = new DirectoryNotFoundException(nameof(directory));
            }
            else if (profile == null)
            {
                await ComposeShowErrorMessage(Resources.Messages.Composition.ProfileNotExists);
                exception = new ArgumentNullException(nameof(profile));
            }

            if (exception != null)
            {
                logbook.Write($"Composition arguments validation failed.", LogLevel.Error, exception);

                return false;
            }

            logbook.Write($"Composition arguments validated.", LogLevel.Debug);

            return true;
        }

        private async Task ComposeShowErrorMessage(string message)
        {
            MessageDialog dialog = new MessageDialog(
                Resources.Labels.General.Error,
                message);

            await dialogAssist.Show(dialog);
        }

        private IEnumerable<string> ComposeGetAllValidFiles(string directory, bool searchSubDirectories)
        {
            logbook.Write($"Getting all valid filepaths for composition.", LogLevel.Debug);

            SearchOption subDirs = searchSubDirectories == true
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

            return Directory
                .GetFiles(directory, "*.*", subDirs)
                .Where(
                    x =>
                        x.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                        || x.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)
                        || x.EndsWith(".doc", StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region Execute composition
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task ExecuteComposition(string directory)
        {
            if (properties.SelectedProfile == null) return;

            await Compose(directory, properties.SelectedProfile);
        }
        #endregion

        #region Execute editable
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteEditable()
        {
            // Null check.

            if (properties.SelectedProfile == null) return;

            logbook.Write($"Changing profile editability.", LogLevel.Debug);

            properties.SelectedProfile.IsEditable = !properties.SelectedProfile.IsEditable;
            options.SaveProfile(properties.SelectedProfile);
        }
        #endregion

        #region Execute edit profile
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteEditProfile()
        {
            // Null check.

            if (properties.SelectedProfile == null) return;

            logbook.Write($"Editing profile.", LogLevel.Information);

            // Create a dialog for editing the profile and show it.

            CompositionProfileDialog? dialog = await ShowProfileEditDialog(properties);

            if (dialog == null) return;

            // If cancelled, just return.

            if (dialog.IsCanceled)
                return;

            // Save the edited profile.

            SaveEditedProfile(properties.SelectedProfile, dialog);

            logbook.Write($"Edited profile saved.", LogLevel.Information);
        }

        private async Task<CompositionProfileDialog?> ShowProfileEditDialog(ICompositionProperties properties)
        {
            // Null check.
            if (properties.SelectedProfile == null) return null;

            CompositionProfileDialog dialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.EditTitle,
                properties.SelectedProfile.ProfileName,
                properties.Profiles.ToList()
            )
            {
                ProfileName = properties.SelectedProfile.ProfileName,
                AddPageNumbers = properties.SelectedProfile.AddPageNumbers
            };

            await dialogAssist.Show(dialog);

            return dialog;
        }

        private void SaveEditedProfile(ICompositionProfile profile, CompositionProfileDialog dialog)
        {
            properties.SelectedProfile = null;

            profile.AddPageNumbers = dialog.AddPageNumbers;
            profile.ProfileName = dialog.ProfileName;

            properties.SelectedProfile = profile;

            options.SaveProfile(profile);
        }
        #endregion

        #region Execute add profile
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAddProfile()
        {
            logbook.Write($"Adding new profile.", LogLevel.Information);

            // Create a new dialog and show it to the user.

            CompositionProfileDialog dialog = await ShowAddProfileDialog(properties);

            // If canceled, just return.

            if (dialog.IsCanceled || string.IsNullOrEmpty(dialog.ProfileName))
            { 
                logbook.Write($"Addition was cancelled or profile name was empty.", LogLevel.Warning);

                return;
            }

            // Create a new profile, save it to the database and immediately select it.

            ICompositionProfile profile = options.CreateProfile(
                dialog.ProfileName,
                dialog.AddPageNumbers,
                true
            );

            SaveAddAndSelect(profile);

            logbook.Write($"Profile added.", LogLevel.Information);
        }

        private async Task<CompositionProfileDialog> ShowAddProfileDialog(ICompositionProperties properties)
        {
            CompositionProfileDialog dialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.NewTitle,
                properties.Profiles.ToList()
            )
            {
                AddPageNumbers = true
            };

            await dialogAssist.Show(dialog);

            return dialog;
        }
        #endregion

        #region Execute delete profile
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteDeleteProfile()
        {
            // Null check.
            if (properties.SelectedProfile == null) return;

            // Create confirmation dialog and show it to the user.

            ConfirmationDialog dialog = await ShowDeleteConfirmationDialog();

            // If cancelled, just return.

            if (dialog.IsCanceled)
                return;

            // Delete selected profile from the database.

            options.DeleteProfile(properties.SelectedProfile);

            logbook.Write(
                $"{nameof(ICompositionProfile)} '{properties.SelectedProfile.ProfileName}' deleted.",
                LogLevel.Information
            );

            // Remove profile from the current list.

            properties.Profiles.Remove(properties.SelectedProfile);
            properties.SelectedProfile = properties.Profiles.Count > 0 ? properties.Profiles[0] : null;
        }

        private async Task<ConfirmationDialog> ShowDeleteConfirmationDialog()
        {
            ConfirmationDialog dialog = new ConfirmationDialog(
                Resources.Labels.Dialogs.Confirmation.DeleteCompositionProfileTitle,
                Resources.Messages.Composition.ProfileDeleteConfirmation
            );

            await dialogAssist.Show(dialog);

            return dialog;
        }
        #endregion

        #region Execute copy profile
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteCopyProfile()
        {
            // Null check.
            if (properties.SelectedProfile == null) return;

            logbook.Write($"Copying profile.", LogLevel.Information);

            CompositionProfileDialog? dialog = await ShowCopyProfileDialog(properties);

            if (dialog == null || dialog.IsCanceled || dialog.ProfileName == null)
            {
                logbook.Write($"Action was cancelled or profile name was null.", LogLevel.Debug);
                return;
            }

            // Create a new profile with copied (and modified) info.

            ICompositionProfile profile = options.CreateProfile(
                dialog.ProfileName,
                dialog.AddPageNumbers,
                true,
                properties.SelectedProfile.Segments?.ToList()
            );

            string? originalName = properties.SelectedProfile.ProfileName;

            SaveAddAndSelect(profile);

            logbook.Write($"Profile copy completed.", LogLevel.Information);
        }

        private async Task<CompositionProfileDialog?> ShowCopyProfileDialog(ICompositionProperties properties)
        {
            // Null check.
            if (properties.SelectedProfile == null) return null;

            CompositionProfileDialog dialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.NewTitle,
                properties.Profiles.ToList()
            )
            {
                ProfileName = $"{properties.SelectedProfile.ProfileName} ({Resources.Labels.General.Copy})",
                AddPageNumbers = properties.SelectedProfile.AddPageNumbers
            };

            await dialogAssist.Show(dialog);

            return dialog;
        }
        #endregion

        #region Execute profile import
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteImportProfile()
        {
            logbook.Write($"Importing composition profile.", LogLevel.Information);
            // Ask the user for filepath to the profile to import.

            string? filePath = pathSelection.OpenFile(
                Resources.UserInput.Descriptions.SelectOpenFile,
                FileType.Profile
            );

            if (string.IsNullOrEmpty(filePath))
            {
                logbook.Write($"Selected path was null or empty.", LogLevel.Warning);

                return;
            }

            ICompositionProfile profile;

            // Import the profile or show error message, if there is an exception.

            try
            {
                profile = GetProfileFromFile(options, filePath);
            }
            catch (ArgumentException ex) when (ex.Message == filePath)
            {
                logbook.Write($"Profile import failed.", LogLevel.Error, ex);

                string content = string.Format(
                    Resources.Messages.Composition.ProfileWrongExtension,
                    Resources.Files.FileExtensions.Profile);

                await ShowProfileImportExportErrorMessage(content);

                return;
            }
            catch (Exception ex)
            {
                logbook.Write($"Profile import failed.", LogLevel.Error, ex);

                await ShowProfileImportExportErrorMessage(Resources.Messages.Composition.ProfileImportFailed);
                return;
            }

            // If a profile with the same name already exists, confirm whether that profile
            // should be overwritten (or the new profile renamed).

            if (await ProfileImportOverwriteIfExistsCheck(properties, profile) == false) return;

            // Save the profile and select it.

            SaveAddAndSelect(profile);

            logbook.Write($"Profile import from {filePath} completed.", LogLevel.Information);

            await ShowProfileImportExportSuccessMessage(Resources.Messages.Composition.ProfileImportSuccess);

            return;
        }

        private ICompositionProfile GetProfileFromFile(ICompositionOptions options, string path)
        {
            logbook.Write($"Attempting import from {path}:", LogLevel.Debug);

            ICompositionProfile? profile = options.ImportProfile(path);

            if (profile == null) throw new IOException("Profile could not be retrieved.");

            profile.IsEditable = true;
            profile.Id = Guid.NewGuid();

            logbook.Write($"Profile import from file successful.", LogLevel.Debug);

            return profile;
        }

        private async Task<bool> ProfileImportOverwriteIfExistsCheck(
            ICompositionProperties properties,
            ICompositionProfile profile)
        {
            if (properties.Profiles.Any(x => x.ProfileName == profile.ProfileName))
            {
                logbook.Write($"Profile with the same name exists. Confirming overwrite.", LogLevel.Debug);

                // Ask, if the existing profile should be overwritten.
                if (await ShowProfileImportOverwriteConfirmation() == false) return false;

                CompositionProfileDialog profileDialog = await ShowProfileImportProfileDialog(properties, profile);

                if (profileDialog.IsCanceled)
                {
                    logbook.Write($"Profile import cancelled.",LogLevel.Debug);

                    return false;
                }

                profile.ProfileName = profileDialog.ProfileName;
                profile.AddPageNumbers = profileDialog.AddPageNumbers;

                logbook.Write($"Profile overwritten or renamed.", LogLevel.Debug);

                return true;
            }

            return true;
        }

        private async Task<bool> ShowProfileImportOverwriteConfirmation()
        {
            ConfirmationDialog confirmationDialog = new ConfirmationDialog(
                    Resources.Labels.Dialogs.Confirmation.CompositionImportProfileExists,
                    Resources.Messages.Composition.ProfileImportExists
                );

            await dialogAssist.Show(confirmationDialog);

            if (confirmationDialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(IDialog)} '{confirmationDialog.DialogTitle}'.",
                    LogLevel.Information
                );

                return false;
            }

            return true;
        }

        private async Task<CompositionProfileDialog> ShowProfileImportProfileDialog(
            ICompositionProperties properties,
            ICompositionProfile profile)
        {
            CompositionProfileDialog profileDialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.ImportTitle,
                properties.Profiles.ToList())
            {
                AddPageNumbers = profile.AddPageNumbers
            };

            await dialogAssist.Show(profileDialog);

            return profileDialog;
        }

        #endregion

        #region Execute profile export
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteExportProfile()
        {
            // Null check.
            if (properties.SelectedProfile == null) return;

            logbook.Write($"Exporting profile.", LogLevel.Information);

            // Ask the user for exported profile file path.

            string? filePath = pathSelection.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.Profile,
                properties.SelectedProfile.ProfileName + Resources.Files.FileExtensions.Profile
            );

            if (string.IsNullOrEmpty(filePath))
            {
                logbook.Write($"Selected path was null or empty.", LogLevel.Warning);

                return;
            }

            // Export the profile or display an error message if the profile could not
            // be exported.

            try
            {
                if (await TryExportProfile(options, properties, filePath) == false) return;
            }
            catch (ArgumentException ex)
            {
                logbook.Write($"Profile export failed.", LogLevel.Error, ex);

                string content = string.Format(
                    Resources.Messages.Composition.ProfileWrongExtension,
                    Resources.Files.FileExtensions.Profile);

                await ShowProfileImportExportErrorMessage(content);

                return;
            }

            logbook.Write($"Profile exported to {filePath}.", LogLevel.Information);

            // Show a success message, if export was successful.

            await ShowProfileImportExportSuccessMessage(Resources.Messages.Composition.ProfileExportSuccess);

            return;
        }

        private async Task<bool> TryExportProfile(
            ICompositionOptions options,
            ICompositionProperties properties,
            string path)
        {
            logbook.Write($"Attempting export to file at {path}.", LogLevel.Debug);

            if (properties.SelectedProfile == null) throw new ArgumentNullException("No profile was selected.");

            bool result = options.ExportProfile(properties.SelectedProfile, path);

            if (result == false)
            {
                logbook.Write($"Profile export failed.", LogLevel.Debug);

                await ShowProfileImportExportErrorMessage(Resources.Messages.Composition.ProfileExportFailed);
            }

            logbook.Write($"Profile exported.", LogLevel.Debug);

            return result;
        }
        #endregion

        #region Import/export execute common private
        private async Task ShowProfileImportExportErrorMessage(string content)
        {
            MessageDialog messageDialog = new MessageDialog(
                    Resources.Labels.General.Error,
                    content
                );

            await dialogAssist.Show(messageDialog);
        }

        private async Task ShowProfileImportExportSuccessMessage(string message)
        {
            MessageDialog successDialog = new MessageDialog(
                Resources.Labels.General.Notification,
                message
            );

            await dialogAssist.Show(successDialog);
        }
        #endregion

        #region Execute add file segment
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAddFileSegment()
        {
            // Null check.
            if (properties.SelectedProfile == null) return;

            if (properties.SelectedProfile.Segments == null)
                properties.SelectedProfile.Segments = new ReorderCollection<ICompositionSegment>();

            logbook.Write($"Adding a file segment.", LogLevel.Information);

            // Ask the user for segment specification.

            CompositionFileSegmentDialog dialog = await ShowFileSegmentAddDialog();

            if (dialog.IsCanceled)
            {
                logbook.Write($"File segment addition cancelled.", LogLevel.Information);

                return;
            }

            // Create a new segment and insert it into the profile.

            ICompositionFile? segment = FileSegmentAddCreateSegment(dialog);

            if (segment == null)
            {
                logbook.Write($"File segment addition failed.", LogLevel.Warning);

                return;
            }

            properties.SelectedProfile.Segments.Add(segment);
            options.SaveProfile(properties.SelectedProfile);

            logbook.Write($"File segment added.", LogLevel.Information);
        }

        private async Task<CompositionFileSegmentDialog> ShowFileSegmentAddDialog()
        {
            CompositionFileSegmentDialog dialog = new CompositionFileSegmentDialog(
                Resources.Labels.Dialogs.CompositionFileSegment.NewTitle
            );

            await dialogAssist.Show(dialog);

            return dialog;
        }

        private ICompositionFile? FileSegmentAddCreateSegment(CompositionFileSegmentDialog dialog)
        {
            if (string.IsNullOrEmpty(dialog.SegmentName))
            {
                logbook.Write($"File segment name was null or empty.", LogLevel.Warning);

                return null;
            }

            ICompositionFile segment = options.CreateFileSegment(dialog.SegmentName);

            segment.NameFromFile = dialog.NameFromFile;
            segment.SearchExpressionString = dialog.SearchTerm;

            if (dialog.ToRemove != null)
            {
                segment.IgnoreExpressionString = dialog.ToRemove;
            }

            segment.MinCount = dialog.MinCount;
            segment.MaxCount = dialog.MaxCount;
            segment.Example = dialog.Example;

            logbook.Write($"File segment created.", LogLevel.Debug);

            return segment;
        }
        #endregion

        #region Execute edit segment
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteEditSegment()
        {
            if (properties.SelectedProfile == null
                || properties.SelectedProfile.Segments == null
                || properties.SelectedProfile.Segments.SelectedItem == null)
                return;

            logbook.Write($"Editing segment.", LogLevel.Information);

            if (properties.SelectedProfile.Segments.SelectedItem is ICompositionFile fileSegment)
            {
                logbook.Write($"Segment is a file segment.", LogLevel.Debug);

                if (await ShowEditSegmentFileSegmentDialog(fileSegment) == false) return;

                options.SaveProfile(properties.SelectedProfile);
            }
            else if (properties.SelectedProfile.Segments.SelectedItem is ICompositionTitle titleSegment)
            {
                logbook.Write($"Segment is a title segment.", LogLevel.Debug);

                if (await ShowEditSegmentTitleSegmentDialog(titleSegment) == false) return;

                options.SaveProfile(properties.SelectedProfile);
            }

            logbook.Write($"Segment edited.", LogLevel.Information);
        }

        private async Task<bool> ShowEditSegmentFileSegmentDialog(ICompositionFile fileSegment)
        {
            CompositionFileSegmentDialog dialog = new CompositionFileSegmentDialog(
                    Resources.Labels.Dialogs.CompositionFileSegment.EditTitle
                );

            dialog.SegmentName = fileSegment.SegmentName;
            dialog.NameFromFile = fileSegment.NameFromFile;
            dialog.SearchTerm = fileSegment.SearchExpressionString;
            dialog.ToRemove = fileSegment.IgnoreExpressionString;
            dialog.MinCount = fileSegment.MinCount;
            dialog.MaxCount = fileSegment.MaxCount;
            dialog.Example = fileSegment.Example;

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(IDialog)} '{dialog.DialogTitle}'.",
                    LogLevel.Information
                );
                return false;
            }

            fileSegment.SegmentName = dialog.SegmentName;
            fileSegment.NameFromFile = dialog.NameFromFile;
            fileSegment.SearchExpressionString = dialog.SearchTerm;
            fileSegment.IgnoreExpressionString = dialog.ToRemove;
            fileSegment.MinCount = dialog.MinCount;
            fileSegment.MaxCount = dialog.MaxCount;
            fileSegment.Example = dialog.Example;

            return true;
        }

        private async Task<bool> ShowEditSegmentTitleSegmentDialog(ICompositionTitle titleSegment)
        {
            CompositionTitleSegmentDialog dialog = new CompositionTitleSegmentDialog(
                    Resources.Labels.Dialogs.CompositionFileSegment.EditTitle
                );

            dialog.SegmentName = titleSegment.SegmentName;

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(IDialog)} '{dialog.DialogTitle}'.",
                    LogLevel.Information
                );
                return false;
            }

            titleSegment.SegmentName = dialog.SegmentName;

            return true;
        }
        #endregion

        #region Execute add title segment
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAddTitleSegment()
        {
            if (properties.SelectedProfile == null) return;

            if (properties.SelectedProfile.Segments == null)
                properties.SelectedProfile.Segments = new ReorderCollection<ICompositionSegment>();

            logbook.Write($"Adding title segment.", LogLevel.Information);

            CompositionTitleSegmentDialog dialog = await ShowAddTitleSegmentDialog();

            if (dialog.IsCanceled || string.IsNullOrEmpty(dialog.SegmentName))
            {
                logbook.Write($"Action cancelled or segment name was null or empty.", LogLevel.Information);

                return;
            }

            ICompositionTitle segment = options.CreateTitleSegment(dialog.SegmentName);

            properties.SelectedProfile.Segments.Add(segment);

            options.SaveProfile(properties.SelectedProfile);

            logbook.Write($"Title segment added.", LogLevel.Information);
        }

        private async Task<CompositionTitleSegmentDialog> ShowAddTitleSegmentDialog()
        {
            CompositionTitleSegmentDialog dialog = new CompositionTitleSegmentDialog(
                Resources.Labels.Dialogs.CompositionFileSegment.NewTitle
            );

            await dialogAssist.Show(dialog);

            return dialog;
        }
        #endregion

        #region Execute delete segment
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteDeleteSegment()
        {
            if (properties.SelectedProfile == null
                || properties.SelectedProfile.Segments == null
                || properties.SelectedProfile.Segments.SelectedItem == null)
                return;

            logbook.Write($"Deleting segment.", LogLevel.Information);

            if (await ShowDeleteSegmentConfirmationDialog() == false)
            {
                logbook.Write($"Segment deletion cancelled.", LogLevel.Information);

                return;
            }

            properties.SelectedProfile.Segments.RemoveSelected();
            options.SaveProfile(properties.SelectedProfile);

            logbook.Write($"Segment deleted.", LogLevel.Information);
        }

        private async Task<bool> ShowDeleteSegmentConfirmationDialog()
        {
            ConfirmationDialog dialog = new ConfirmationDialog(
                Resources.Labels.Dialogs.Confirmation.DeleteCompositionSegmentTitle,
                Resources.Messages.Composition.SegmentDeleteConfirmation
            );

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Common private
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        private void SaveAddAndSelect(ICompositionProfile profile)
        {
            options.SaveProfile(profile);
            properties.Profiles.Add(profile);
            properties.SelectedProfile = profile;
        }
        #endregion
    }
}
