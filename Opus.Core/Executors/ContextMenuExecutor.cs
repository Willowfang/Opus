using WF.LoggingLib;
using WF.PdfLib.Common;
using WF.PdfLib.Services;
using WF.PdfLib.Services.Data;
using Opus.Core.Wrappers;
using Opus.Services.Configuration;
using Opus.Services.Data;
using Opus.Services.Data.Composition;
using Opus.Services.Extensions;
using Opus.Services.Implementation.Data.Extraction;
using Opus.Services.Implementation.Logging;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Input;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Opus.Core.Executors
{
    /// <summary>
    /// This class wraps together pdf-manipulation and other types of services to run them
    /// when an action is requested in the context-menu environment.
    /// </summary>
    public class ContextMenuExecutor : LoggingCapable<ContextMenuExecutor>, IContextMenu
    {
        // Dependency injection services
        private readonly IDialogAssist dialogAssist;
        private readonly IConfiguration configuration;
        private readonly IPathSelection input;
        private readonly IComposer composer;
        private readonly ICompositionOptions compositionOptions;
        private readonly IExtractionExecutor extractionExecutor;
        private readonly IPdfAConvertService pdfAConvertService;
        private readonly ISignatureExecutor signatureExecutor;
        private readonly IBookmarkService bookmarkService;
        private readonly IAnnotationService annotationService;

        private static Mutex mutex = new Mutex(true, "{59FCB8B2-6919-44EF-A717-55DE4C95319E}");

        /// <summary>
        /// Create a new context menu action executor.
        /// </summary>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="input">Service for taking user input for choosing a file or folder path.</param>
        /// <param name="composer">Service for composing a new pdf-document.</param>
        /// <param name="compositionOptions">Options for composition.</param>
        /// <param name="extractionExecutor">Collection of smaller services for extracting bookmarks from a document.</param>
        /// <param name="pdfAConverterService">Service for converting a pdf product to pdf/a-format.</param>
        /// <param name="signatureExecutor">Service for removing digital signatures from documents.</param>
        /// <param name="bookmarkService">Service for finding bookmarks in a pdf-document.</param>
        /// <param name="annotationService">Service for finding and manipulating annotations in a pdf-document.</param>
        /// <param name="logbook">Logging service.</param>
        public ContextMenuExecutor(
            IDialogAssist dialogAssist,
            IConfiguration configuration,
            IPathSelection input,
            IComposer composer,
            ICompositionOptions compositionOptions,
            IExtractionExecutor extractionExecutor,
            IPdfAConvertService pdfAConverterService,
            ISignatureExecutor signatureExecutor,
            IBookmarkService bookmarkService,
            IAnnotationService annotationService,
            ILogbook logbook
        ) : base(logbook)
        {
            // Assign DI services
            this.dialogAssist = dialogAssist;
            this.configuration = configuration;
            this.input = input;
            this.composer = composer;
            this.compositionOptions = compositionOptions;
            this.extractionExecutor = extractionExecutor;
            this.pdfAConvertService = pdfAConverterService;
            this.signatureExecutor = signatureExecutor;
            this.bookmarkService = bookmarkService;
            this.annotationService = annotationService;
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
                await CreateWorkingCopy(arguments);
            else if (operation == Resources.ContextMenu.Arguments.Compose)
                await Compose(arguments);
            else if (operation == Resources.ContextMenu.Arguments.ConvertToPdfA)
                await ConvertToPdfA(arguments);
            else
                return;
        }

        /// <summary>
        /// Convert the given pdf-document to pdf/a.
        /// </summary>
        /// <param name="arguments">Arguments containing info on the file.</param>
        /// <returns>An awaitable task.</returns>
        protected async Task ConvertToPdfA(string[] arguments)
        {
            // If the system does not contain required software for
            // pdf/a -conversion, inform the user with a dialog and return.

            if (configuration.ExtractionPdfADisabled == true)
            {
                MessageDialog toolsDialog = new MessageDialog(
                    Resources.Labels.General.Notification,
                    Resources.Messages.ContextMenu.PdfADisabled
                );
                await dialogAssist.Show(toolsDialog);
                return;
            }

            // If the arguments do not contain a path to a file, return, or if there
            // are more than two arguments.

            if (arguments.Length != 2)
                return;

            // Ask the user for a save path.

            string filePath = arguments[1];
            string destinationPath = input.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.PDF
            );

            if (destinationPath == null || filePath == null)
                return;

            // Try to copy the original file to destination path. If there is an access error,
            // inform the user and return.

            try
            {
                string originalPath = Path.GetFullPath(filePath);
                string selectedPath = Path.GetFullPath(destinationPath);

                if (originalPath.Equals(selectedPath, StringComparison.OrdinalIgnoreCase) == false)
                    File.Copy(filePath, destinationPath, true);
            }
            catch (UnauthorizedAccessException)
            {
                MessageDialog unauthorized = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.General.ErrorFileInUse
                );
                await dialogAssist.Show(unauthorized);
                return;
            }

            // Start the conversion process on the copied file at the destination path.

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var result = ShowProgress(tokenSource);

            logbook.Write($"Starting conversion to pdf/a.", LogLevel.Information);

            bool success = await pdfAConvertService.Convert(
                new FileInfo(destinationPath),
                new DirectoryInfo(Path.GetDirectoryName(destinationPath)),
                token
            );

            // If conversion fails, show a dialog to inform the user and return.

            if (success == false)
            {
                string content = $"{Resources.Messages.Extraction.PdfAConversionError}.";
                MessageDialog message = new MessageDialog(
                    Resources.Labels.General.Notification,
                    content
                );
                await dialogAssist.Show(message);
                tokenSource.Cancel();
                return;
            }

            // If the conversion succeeds, show progress 100 percent and exit on user acknowledgement.

            logbook.Write($"Pdf/a -conversion completed succesfully.", LogLevel.Information);

            result.progress.Report(new ProgressReport(100, ProgressPhase.Finished));
            await result.dialog;
        }

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

            // Ask the user for the destination directory path.

            string filePath = arguments[1];
            string fileDirectory = input.OpenDirectory(
                Resources.UserInput.Descriptions.SelectSaveFolder
            );

            if (fileDirectory == null)
                return;

            // Get relevant bookmarks. Either all of the bookmarks, or just the ones
            // corresponding with the second argument.

            IList<ILeveledBookmark> ranges;

            if (arguments.Length == 2)
                ranges = await GetBookmarks(filePath);
            else
                ranges = await GetBookmarks(filePath, arguments[2]);

            // Perform the extraction.

            FileAndBookmarksStorage storage = new FileAndBookmarksStorage(filePath);
            foreach (ILeveledBookmark range in ranges)
            {
                storage.Bookmarks.Add(
                    new FileAndBookmarkWrapper(range, filePath) { IsSelected = true }
                );
            }

            logbook.Write($"Starting bookmark extraction.", LogLevel.Information);

            await extractionExecutor.Save(
                new DirectoryInfo(fileDirectory),
                new List<FileAndBookmarkWrapper>(storage.Bookmarks)
            );

            logbook.Write($"Bookmark extraction finished.", LogLevel.Information);
        }

        /// <summary>
        /// Create a work copy of the selected file.
        /// </summary>
        /// <param name="arguments">Arguments passed to the application.</param>
        /// <returns>An awaitable task.</returns>
        protected async Task CreateWorkingCopy(string[] arguments)
        {
            // If there is an incorrect number of arguments, return.

            if (arguments.Length != 2)
                return;
            if (string.IsNullOrEmpty(arguments[1]))
                return;

            // The destination directory is the same as source directory.

            string filePath = arguments[1];
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(filePath)!);

            // Prepare for signature removal.

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            ProgressDialog dialog = new ProgressDialog(string.Empty, tokenSource)
            {
                TotalPercent = 0,
                Phase = Resources.Operations.PhaseNames.Unassigned
            };

            Task showProgress = dialogAssist.Show(dialog);

            IEnumerable<FileStorage> file = new List<FileStorage>() { new FileStorage(filePath) };

            // Remove signature.

            logbook.Write($"Starting signature removal.", LogLevel.Information);

            IList<FileInfo> created = await signatureExecutor.Remove(file, directory, tokenSource);

            // Flatten redactions.

            List<Task> tasks = new List<Task>();
            foreach (FileInfo redFile in created)
            {
                tasks.Add(annotationService.FlattenRedactions(redFile.FullName));
            }

            // When all is ready, inform the user.

            await Task.WhenAll(tasks);

            logbook.Write($"Signature removal finished.", LogLevel.Information);

            dialog.TotalPercent = 100;
            dialog.Phase = Resources.Operations.PhaseNames.Finished;

            await showProgress;
        }

        /// <summary>
        /// Compose a document from a folder.
        /// </summary>
        /// <param name="arguments">Arguments passed to the application.</param>
        /// <returns>An awaitable task.</returns>
        protected async Task Compose(string[] arguments)
        {
            // If there is an incorrect number of arguments, return.

            if (arguments.Length != 2)
                return;
            if (string.IsNullOrEmpty(arguments[1]))
                return;

            string directoryPath = arguments[1];

            // Let user select the correct profile for the composition.

            IList<ICompositionProfile> profiles = compositionOptions.GetProfiles();
            CompositionProfileSelectionDialog dialog = new CompositionProfileSelectionDialog(
                Resources.Labels.Dialogs.CompositionProfileSelection.Title,
                profiles
            )
            {
                SelectedProfile = profiles.FirstOrDefault(x => x.Id == configuration.DefaultProfile)
            };

            await dialogAssist.Show(dialog);

            // If the user cancels, return.

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

            // Compose the document.

            await composer.Compose(
                directoryPath,
                dialog.SelectedProfile,
                configuration.CompositionDeleteConverted,
                configuration.CompositionSearchSubDirectories
            );

            logbook.Write($"Composition finished.", LogLevel.Information);
        }

        /// <summary>
        /// Get all bookmarks of a document.
        /// </summary>
        /// <param name="filePath">Path of the document.</param>
        /// <returns>Bookmarks contained in the document.</returns>
        private async Task<IList<ILeveledBookmark>> GetBookmarks(string filePath)
        {
            return await bookmarkService.FindBookmarks(filePath);
        }

        /// <summary>
        /// Get all bookmarks with a given prefix.
        /// </summary>
        /// <param name="filePath">Path of the document to get the bookmarks from.</param>
        /// <param name="preFix">Prefix to search for.</param>
        /// <returns>Found bookmarks.</returns>
        private async Task<IList<ILeveledBookmark>> GetBookmarks(string filePath, string preFix)
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

        /// <summary>
        /// Show progress dialog to the user.
        /// </summary>
        /// <param name="cancelSource">Cancellation token source.</param>
        /// <returns>Dialog shown and the progress container.</returns>
        private (Task dialog, IProgress<ProgressReport> progress) ShowProgress(
            CancellationTokenSource cancelSource
        )
        {
            ProgressDialog dialog = new ProgressDialog(null, cancelSource)
            {
                TotalPercent = 0,
                Phase = ProgressPhase.Unassigned.GetResourceString()
            };
            Progress<ProgressReport> progress = new Progress<ProgressReport>(report =>
            {
                dialog.TotalPercent = report.Percentage;
                dialog.Phase = report.CurrentPhase.GetResourceString();
                dialog.Part = report.CurrentItem;
            });

            return (dialogAssist.Show(dialog), progress);
        }
    }
}
