using CX.LoggingLib;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
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
    public class ContextMenuExecutor : LoggingCapable<ContextMenuExecutor>, IContextMenu
    {
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

        public async Task Run(string[] arguments)
        {
            string operation = arguments[0];

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

        private async Task ConvertToPdfA(string[] arguments)
        {
            if (configuration.ExtractionPdfADisabled == true)
            {
                MessageDialog toolsDialog = new MessageDialog(
                    Resources.Labels.General.Notification,
                    Resources.Messages.ContextMenu.PdfADisabled
                );
                await dialogAssist.Show(toolsDialog);
                return;
            }

            if (arguments.Length != 2)
                return;

            string filePath = arguments[1];
            string destinationPath = input.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.PDF
            );

            if (destinationPath == null || filePath == null)
                return;

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

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var result = ShowProgress(tokenSource);

            logbook.Write($"Starting conversion to pdf/a.", LogLevel.Information);

            bool success = await pdfAConvertService.Convert(
                new FileInfo(destinationPath),
                new DirectoryInfo(Path.GetDirectoryName(destinationPath)),
                token
            );

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

            logbook.Write($"Pdf/a -conversion completed succesfully.", LogLevel.Information);

            result.progress.Report(new ProgressReport(100, ProgressPhase.Finished));
            await result.dialog;
        }

        private async Task ExtractFile(string[] arguments)
        {
            if (arguments.Length < 2 || arguments.Length > 3)
                return;

            string filePath = arguments[1];
            string fileDirectory = input.OpenDirectory(
                Resources.UserInput.Descriptions.SelectSaveFolder
            );

            if (fileDirectory == null)
                return;

            IList<ILeveledBookmark> ranges;

            if (arguments.Length == 2)
                ranges = await GetBookmarks(filePath);
            else
                ranges = await GetBookmarks(filePath, arguments[2]);

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

        private async Task CreateWorkingCopy(string[] arguments)
        {
            if (arguments.Length != 2)
                return;
            if (string.IsNullOrEmpty(arguments[1]))
                return;

            string filePath = arguments[1];
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(filePath)!);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            ProgressDialog dialog = new ProgressDialog(string.Empty, tokenSource)
            {
                TotalPercent = 0,
                Phase = Resources.Operations.PhaseNames.Unassigned
            };

            Task showProgress = dialogAssist.Show(dialog);

            IEnumerable<FileStorage> file = new List<FileStorage>() { new FileStorage(filePath) };

            logbook.Write($"Starting signature removal.", LogLevel.Information);

            IList<FileInfo> created = await signatureExecutor.Remove(file, directory, tokenSource);

            List<Task> tasks = new List<Task>();
            foreach (FileInfo redFile in created)
            {
                tasks.Add(annotationService.FlattenRedactions(redFile.FullName));
            }

            await Task.WhenAll(tasks);

            logbook.Write($"Signature removal finished.", LogLevel.Information);

            dialog.TotalPercent = 100;
            dialog.Phase = Resources.Operations.PhaseNames.Finished;

            await showProgress;
        }

        private async Task Compose(string[] arguments)
        {
            if (arguments.Length != 2)
                return;
            if (string.IsNullOrEmpty(arguments[1]))
                return;

            string directoryPath = arguments[1];

            IList<ICompositionProfile> profiles = compositionOptions.GetProfiles();
            CompositionProfileSelectionDialog dialog = new CompositionProfileSelectionDialog(
                Resources.Labels.Dialogs.CompositionProfileSelection.Title,
                profiles
            )
            {
                SelectedProfile = profiles.FirstOrDefault(x => x.Id == configuration.DefaultProfile)
            };

            await dialogAssist.Show(dialog);

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

            await composer.Compose(
                directoryPath,
                dialog.SelectedProfile,
                configuration.CompositionDeleteConverted,
                configuration.CompositionSearchSubDirectories
            );

            logbook.Write($"Composition finished.", LogLevel.Information);
        }

        private async Task<IList<ILeveledBookmark>> GetBookmarks(string filePath)
        {
            return await bookmarkService.FindBookmarks(filePath);
        }

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
