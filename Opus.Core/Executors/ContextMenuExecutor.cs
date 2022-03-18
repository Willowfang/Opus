using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using Opus.Core.Wrappers;
using Opus.Services.Configuration;
using Opus.Services.Data;
using Opus.Services.Data.Composition;
using Opus.Services.Extensions;
using Opus.Services.Implementation.StaticHelpers;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Input;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opus.Core.Executors
{
    public class ContextMenuExecutor : IContextMenu
    {
        private IDialogAssist dialogAssist;
        private IConfiguration configuration;
        private ISignatureOptions signatureOptions;
        private IManipulator manipulator;
        private IPathSelection input;
        private IComposerFactory composerFactory;
        private ICompositionOptions compositionOptions;
        private IExtractionExecutor extractionExecutor;
        private IPdfAConverter pdfAConverter;

        public ContextMenuExecutor(IDialogAssist dialogAssist, IConfiguration configuration,
            ISignatureOptions signatureOptions, IManipulator manipulator,
            IPathSelection input, IComposerFactory composerFactory, ICompositionOptions compositionOptions,
            IExtractionExecutor extractionExecutor, IPdfAConverter pdfAConverter)
        {
            this.dialogAssist = dialogAssist;
            this.configuration = configuration;
            this.signatureOptions = signatureOptions;
            this.manipulator = manipulator;
            this.input = input;
            this.composerFactory = composerFactory;
            this.compositionOptions = compositionOptions;
            this.extractionExecutor = extractionExecutor;
            this.pdfAConverter = pdfAConverter;
        }

        public async Task Run(string[] arguments)
        {
            string operation = arguments[0];

            if (operation == Resources.ContextMenu.Arguments.ExtractFile)
                await ExtractFile(arguments);
            else if (operation == Resources.ContextMenu.Arguments.RemoveSignature)
                await RemoveSignature(arguments);
            else if (operation == Resources.ContextMenu.Arguments.Compose)
                await Compose(arguments);
            else if (operation == Resources.ContextMenu.Arguments.ConvertToPdfA)
                await ConvertToPdfA(arguments);
            else
                return;
        }

        private async Task ConvertToPdfA(string[] arguments)
        {
            if (arguments.Length != 2)
                return;

            string filePath = arguments[1];
            string destinationPath = input.SaveFile(Resources.UserInput.Descriptions.SelectSaveFile);

            if (destinationPath == null || filePath == null) return;

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var result = ShowProgress(tokenSource);

            bool success = await pdfAConverter.Convert(new DirectoryInfo(filePath), new DirectoryInfo(destinationPath));

            if (success == false)
            {
                string content = $"{Resources.Messages.Extraction.PdfAConversionError}.";
                MessageDialog message = new MessageDialog(Resources.Labels.General.Notification, content);
                await dialogAssist.Show(message);
                tokenSource.Cancel();
                return;
            }

            result.progress.Report(new ProgressReport(100, ProgressPhase.Finished));
            await result.dialog;
        }

        private async Task ExtractFile(string[] arguments)
        {
            if (arguments.Length < 2 || arguments.Length > 3)
                return;

            string filePath = arguments[1];
            string fileDirectory = input.OpenDirectory(Resources.UserInput.Descriptions.SelectSaveFolder);

            if (fileDirectory == null) return;

            IList<ILeveledBookmark> ranges;

            if (arguments.Length == 2)
                ranges = GetBookmarks(filePath);
            else
                ranges = GetBookmarks(filePath, arguments[2]);

            FileAndBookmarksStorage storage = new FileAndBookmarksStorage(filePath);
            foreach (ILeveledBookmark range in ranges)
            {
                storage.Bookmarks.Add(new BookmarkStorage(range) { IsSelected = true });
            }

            await extractionExecutor.Save(new DirectoryInfo(fileDirectory), 
                new List<FileAndBookmarksStorage> { storage });
        }

        private async Task RemoveSignature(string[] arguments)
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
            Task unsign = manipulator.RemoveSignatureAsync(filePath, directory, signatureOptions.Suffix);

            await unsign;

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
                Resources.Labels.Dialogs.CompositionProfileSelection.Title, profiles)
            {
                SelectedProfile = profiles.FirstOrDefault(x => x.Id == configuration.DefaultProfile)
            };

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled) return;

            IComposer composer = composerFactory.Create();
            await composer.Compose(directoryPath, dialog.SelectedProfile, configuration.CompositionDeleteConverted,
                configuration.CompositionSearchSubDirectories);
        }

        private IList<ILeveledBookmark> GetBookmarks(string filePath)
        {
            IList<ILeveledBookmark> foundBookMarks = manipulator.FindBookmarks(filePath);
            return foundBookMarks;
        }

        private IList<ILeveledBookmark> GetBookmarks(string filePath, string preFix)
        {
            List<ILeveledBookmark> selected = new List<ILeveledBookmark>();
            foreach (ILeveledBookmark bookmark in manipulator.FindBookmarks(filePath))
            {
                if (bookmark.Title.ToLower().StartsWith(preFix.ToLower()))
                {
                    selected.Add(bookmark);
                }
            }

            return selected;
        }

        private (Task dialog, IProgress<ProgressReport> progress) ShowProgress(CancellationTokenSource cancelSource)
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
