using Opus.Actions.Services.Compose;
using Opus.Actions.Services.Extract;
using Opus.Actions.Services.WorkCopy;
using Opus.Common.Wrappers;
using Opus.Common.Services.Configuration;
using Opus.Common.Services.Data.Composition;
using Opus.Common.Logging;
using Opus.Common.Dialogs;
using Opus.Common.Services.Input;
using Opus.Common.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WF.LoggingLib;
using WF.PdfLib.Common;
using WF.PdfLib.Services;
using WF.PdfLib.Services.Data;
using Opus.Common.Services.ContextMenu;
using Opus.Common.Progress;

namespace Opus.Initialize
{
    internal class ContextMenu : LoggingCapable<ContextMenu>, IContextMenu
    {
        private readonly IExtractionSupportProperties extractionProperties;
        private readonly IExtractionSupportMethods extractionMethods;
        private readonly IBookmarkService bookmarkService;
        private readonly IWorkCopyMethods workCopyMethods;
        private readonly ICompositionOptions compositionOptions;
        private readonly ICompositionMethods compositionMethods;
        private readonly IDialogAssist dialogAssist;
        private readonly IConfiguration configuration;
        private readonly IPathSelection pathSelection;
        private readonly IPdfAConvertService pdfAConvertService;

        public ContextMenu(
            IExtractionSupportProperties extractionProperties,
            IExtractionSupportMethods extractionMethods,
            IBookmarkService bookmarkService,
            IWorkCopyMethods workCopyMethods,
            ICompositionOptions compositionOptions,
            ICompositionMethods compositionMethods,
            IDialogAssist dialogAssist,
            IConfiguration configuration,
            IPathSelection pathSelection,
            IPdfAConvertService pdfAConvertService,
            ILogbook logbook) : base(logbook)
        {
            this.extractionProperties = extractionProperties;
            this.extractionMethods = extractionMethods;
            this.bookmarkService = bookmarkService;
            this.workCopyMethods = workCopyMethods;
            this.compositionOptions = compositionOptions;
            this.compositionMethods = compositionMethods;
            this.dialogAssist = dialogAssist;
            this.configuration = configuration;
            this.pathSelection = pathSelection;
            this.pdfAConvertService = pdfAConvertService;
        }

        /// <summary>
        /// Run the requested action.
        /// </summary>
        /// <param name="arguments">Arguments containing info on the requested
        /// action and other required info.</param>
        /// <returns>An awaitable task.</returns>
        public async Task Run(string[] arguments)
        {
            string operation = arguments[0];

            // Start the correct operation based on the first argument.
            // If the first arg is not a known action, then just return.
            if (operation == Resources.ContextMenu.Arguments.ExtractFile)
                await ExtractFile(arguments);
            else if (operation == Resources.ContextMenu.Arguments.WorkingCopy)
                await CreateWorkCopy(arguments);
            else if (operation == Resources.ContextMenu.Arguments.WorkingCopyMultiple)
                await CreateWorkCopyFolder(arguments);
            else if (operation == Resources.ContextMenu.Arguments.Compose)
                await Compose(arguments);
            else if (operation == Resources.ContextMenu.Arguments.ConvertToPdfA)
                await ConvertToPdfA(arguments);
            else
                return;
        }

        #region Extract file
        /// <summary>
        /// Extract bookmarks from a pdf-file.
        /// </summary>
        /// <param name="arguments">Arguments passed into the application.</param>
        /// <returns>An awaitable task.</returns>
        protected async Task ExtractFile(string[] arguments)
        {
            // If there is an incorrect number of arguments, return.

            if (arguments.Length < 2 || arguments.Length > 3)
                return;

            string filePath = arguments[1];

            if (File.Exists(filePath) == false)
            {
                logbook.Write(
                    $"Attempted to extract bookmarks from a non-existent file at {filePath}",
                    LogLevel.Error);

                return;
            }

            // Get relevant bookmarks. Either all of the bookmarks, or just the ones
            // corresponding with the second argument.

            IList<ILeveledBookmark> ranges = await ExtractFileGetBookmarks(filePath, arguments);

            if (ranges == null || ranges.Count < 1)
            {
                await ShowBookmarksNotFoundMessage(arguments);
                return;
            }

            ExtractFileAddWrappers(ranges, filePath, extractionProperties);

            logbook.Write($"Starting bookmark extraction.", LogLevel.Information);

            await extractionMethods.ExecuteSaveSeparate();

            logbook.Write($"Bookmark extraction finished.", LogLevel.Information);
        }

        private async Task ShowBookmarksNotFoundMessage(string[] arguments)
        {
            string message = arguments.Length == 2
                ? Resources.Messages.Extraction.NoBookmarksFound
                : Resources.Messages.Extraction.NoBookmarksWithPrefixFound;

            MessageDialog dialog = new MessageDialog(
                Resources.Labels.General.Notification,
                message);

            await dialogAssist.Show(dialog);
        }

        private async Task<IList<ILeveledBookmark>> ExtractFileGetBookmarks(string filePath, string[] arguments)
        {
            IList<ILeveledBookmark> ranges;

            if (arguments.Length == 2)
                ranges = await bookmarkService.FindBookmarks(filePath);
            else
                ranges = await ExtractFileGetBookmarksWithPrefix(filePath, arguments[2]);

            return ranges;
        }
        
        private async Task<IList<ILeveledBookmark>> ExtractFileGetBookmarksWithPrefix(string filePath, string preFix)
        {
            List<ILeveledBookmark> selected = new List<ILeveledBookmark>();
            foreach (ILeveledBookmark bookmark in await bookmarkService.FindBookmarks(filePath))
            {
                if (bookmark.Title.ToLower().StartsWith(preFix.ToLower()))
                {
                    selected.Add(bookmark);
                }
            }

            return selected;
        }

        private static void ExtractFileAddWrappers(
            IList<ILeveledBookmark> ranges,
            string filePath,
            IExtractionSupportProperties properties)
        {
            for (int i = 0; i < ranges.Count; i++)
            {
                properties.Bookmarks.Add(
                    new FileAndBookmarkWrapper(ranges[i], filePath, i + 1) { IsSelected = true }
                );
            }
        }
        #endregion

        #region Create work copy
        /// <summary>
        /// Create a work copy of the selected file.
        /// </summary>
        /// <param name="arguments">Arguments passed to the application.</param>
        /// <returns>An awaitable task.</returns>
        protected async Task CreateWorkCopy(string[] arguments)
        {
            // If there is an incorrect number of arguments, return.

            logbook.Write(
                $"Work copy creation called with arguments: {arguments}.", LogLevel.Debug);

            if (arguments.Length != 2)
                return;

            if (string.IsNullOrEmpty(arguments[1]))
                return;

            // The destination directory is the same as source directory.

            string filePath = arguments[1];

            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(filePath));

            await workCopyMethods.Remove(new List<FileStorage>() { new FileStorage(filePath) }, directory);
        }
        #endregion

        #region Create work copy folder
        /// <summary>
        /// Create work copies from all pdf-files in a given folder and its subfolders.
        /// </summary>
        /// <param name="arguments">Arguments passed to the application.</param>
        /// <returns>An awaitable task.</returns>
        protected async Task CreateWorkCopyFolder(string[] arguments)
        {
            // If there is an incorrect number of arguments, return.

            if (arguments.Length != 2)
                return;

            if (string.IsNullOrEmpty(arguments[1]))
                return;

            // Create a new subdirectory for products.

            string directoryPath = arguments[1];

            DirectoryInfo directory = WorkCopyFolderCreateProductSubDirectory(directoryPath);

            // Retrieve pdfs as storage instances.

            List<FileStorage> pdfStorages = WorkCopyFolderGetPdfStoragesFromFiles(directoryPath);

            // Remove signatures.

            await workCopyMethods.Remove(pdfStorages, directory);
        }

        private static DirectoryInfo WorkCopyFolderCreateProductSubDirectory(string originalDirectoryPath)
        {
            string subDirectoryPath = Path.Combine(
                originalDirectoryPath,
                Resources.ContextMenu.FolderNames.WorkCopyMultipleFolder);

            while (Directory.Exists(subDirectoryPath))
            {
                subDirectoryPath = subDirectoryPath + " " + Resources.Labels.General.Copy.ToLower();
            }

            DirectoryInfo directory = new DirectoryInfo(subDirectoryPath);

            directory.Create();

            return directory;
        }

        private List<FileStorage> WorkCopyFolderGetPdfStoragesFromFiles(string originalDirectoryPath)
        {
            DirectoryInfo searchDir = new DirectoryInfo(originalDirectoryPath);

            // Find all pdf files in the directory and its subdirectories.

            IEnumerable<FileInfo> pdfs = searchDir.GetFiles("*.pdf", SearchOption.AllDirectories);

            List<FileStorage> storages = new List<FileStorage>();
            foreach (FileInfo pdf in pdfs)
            {
                storages.Add(new FileStorage(pdf.FullName));
            }

            return storages;
        }
        #endregion

        #region Compose
        /// <summary>
        /// Compose a document from a folder.
        /// </summary>
        /// <param name="arguments">Arguments passed to the application.</param>
        /// <returns>An awaitable task.</returns>
        protected async Task Compose(string[] arguments)
        {
            if (arguments.Length != 2)
                return;

            if (string.IsNullOrEmpty(arguments[1]))
                return;

            string directoryPath = arguments[1];

            CompositionProfileSelectionDialog dialog = await ComposeShowSelectProfileDialog();

            if (dialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation was requested at {nameof(IDialog)} '{dialog.DialogTitle}'.",
                    LogLevel.Information
                );

                return;
            }

            logbook.Write(
                $"Starting composition with {nameof(ICompositionProfile)} '{dialog.SelectedProfile.ProfileName}'.",
                LogLevel.Information
            );

            await compositionMethods.Compose(directoryPath, dialog.SelectedProfile);
        }

        private async Task<CompositionProfileSelectionDialog> ComposeShowSelectProfileDialog()
        {
            IList<ICompositionProfile> profiles = compositionOptions.GetProfiles();
            CompositionProfileSelectionDialog dialog = new CompositionProfileSelectionDialog(
                Resources.Labels.Dialogs.CompositionProfileSelection.Title,
                profiles
            )
            {
                SelectedProfile = profiles.FirstOrDefault(x => x.Id == configuration.DefaultProfile)
            };

            await dialogAssist.Show(dialog);

            return dialog;
        }
        #endregion

        #region Convert to pdf/a
        /// <summary>
        /// Convert the given pdf-document to pdf/a.
        /// </summary>
        /// <param name="arguments">Arguments containing info on the file.</param>
        /// <returns>An awaitable task.</returns>
        protected async Task ConvertToPdfA(string[] arguments)
        {
            // If the system does not contain required software for
            // pdf/a -conversion, inform the user with a dialog and return.

            if (await ConvertToPdfACheckForRequirements() == false) return;

            // If the arguments do not contain a path to a file or if there
            // are more than two arguments, return.

            if (arguments.Length != 2 || string.IsNullOrEmpty(arguments[1]))
                return;

            string filePath = arguments[1];

            string destinationPath = pathSelection.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.PDF
            );

            if (string.IsNullOrEmpty(destinationPath)) return;

            bool copySuccess = await ConvertToPdfATryCopyOriginalToDestination(filePath, destinationPath);

            if (copySuccess == false) return;

            ProgressTracker progress = new ProgressTracker(100, dialogAssist);

            bool convertSuccess = await ConvertToPdfATryConvert(destinationPath, progress);

            if (convertSuccess == false) return;

            logbook.Write(
                "Pdf/a-conversion was completed successfully.", LogLevel.Information);

            progress.Update(100, ProgressPhase.Finished);

            await progress.Show;
        }

        private async Task<bool> ConvertToPdfATryConvert(string destinationPath, ProgressTracker tracker)
        {
            logbook.Write($"Starting conversion to pdf/a.", LogLevel.Information);

            bool success = await pdfAConvertService.Convert(
                new FileInfo(destinationPath),
                new DirectoryInfo(Path.GetDirectoryName(destinationPath)),
                tracker.Token
            );

            // If conversion fails, show a dialog to inform the user and return.

            if (success == false)
            {
                logbook.Write(
                    "Pdf/a-conversion failed.", LogLevel.Error);

                string content = $"{Resources.Messages.Extraction.PdfAConversionError}.";

                MessageDialog message = new MessageDialog(
                    Resources.Labels.General.Notification,
                    content
                );

                await dialogAssist.Show(message);

                tracker.Cancel();
            }

            return success;
        }

        private async Task<bool> ConvertToPdfATryCopyOriginalToDestination(string original, string destination)
        {
            try
            {
                string originalPath = Path.GetFullPath(original);

                string selectedPath = Path.GetFullPath(destination);

                if (originalPath.Equals(selectedPath, StringComparison.OrdinalIgnoreCase) == false)
                    File.Copy(original, destination, true);

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                logbook.Write(
                    "File could not be accessed when attempting to convert to pdf/a.", LogLevel.Error, ex);

                MessageDialog unauthorized = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.General.ErrorFileInUse
                );

                await dialogAssist.Show(unauthorized);

                return false;
            }
            catch (Exception ex)
            {
                logbook.Write(
                    "An error was encountered while trying to convert to pdf/a.", LogLevel.Error, ex);

                MessageDialog error = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.General.GeneralErrorMessage
                );

                await dialogAssist.Show(error);

                return false;
            }
        }

        private async Task<bool> ConvertToPdfACheckForRequirements()
        {
            if (configuration.ExtractionPdfADisabled == true)
            {
                MessageDialog toolsDialog = new MessageDialog(
                    Resources.Labels.General.Notification,
                    Resources.Messages.ContextMenu.PdfADisabled
                );

                await dialogAssist.Show(toolsDialog);
            }

            return !configuration.ExtractionPdfADisabled;
        }
        #endregion
    }
}
