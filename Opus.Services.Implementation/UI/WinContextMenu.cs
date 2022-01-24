using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
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

namespace Opus.Services.Implementation.UI
{
    public class WinContextMenu : IContextMenu
    {
        private IDialogAssist dialogAssist;
        private IConfiguration configuration;
        private ISignatureOptions signatureOptions;
        private IManipulator manipulator;
        private IPathSelection input;
        private IComposerFactory composerFactory;
        private ICompositionOptions compositionOptions;

        public WinContextMenu(IDialogAssist dialogAssist, IConfiguration configuration,
            ISignatureOptions signatureOptions, IManipulator manipulator,
            IPathSelection input, IComposerFactory composerFactory, ICompositionOptions compositionOptions)
        {
            this.dialogAssist = dialogAssist;
            this.configuration = configuration;
            this.signatureOptions = signatureOptions;
            this.manipulator = manipulator;
            this.input = input;
            this.composerFactory = composerFactory;
            this.compositionOptions = compositionOptions;
        }

        public async Task Run(string[] arguments)
        {
            string operation = arguments[0];

            if (operation == Resources.ContextMenu.Arguments.ExtractFile)
                await ExtractFile(arguments);
            else if (operation == Resources.ContextMenu.Arguments.ExtractDirectory)
                await ExtractDirectory(arguments);
            else if (operation == Resources.ContextMenu.Arguments.RemoveSignature)
                await RemoveSignature(arguments);
            else if (operation == Resources.ContextMenu.Arguments.Compose)
                await Compose(arguments);
            else
                return;
        }

        private async Task ExtractFile(string[] arguments)
        {
            if (arguments.Length < 2 || arguments.Length > 3)
                return;

            string filePath = arguments[1];
            string? fileDirectory = Path.GetDirectoryName(filePath);

            if (fileDirectory == null)
                return;

            string dir = Directory.CreateDirectory(Path.Combine(fileDirectory,
                Path.GetFileNameWithoutExtension(filePath) + "_" +
                Resources.DefaultValues.DefaultValues.UnsignedSuffix)).FullName;

            IList<ILeveledBookmark> ranges;

            if (arguments.Length == 2)
                ranges = GetBookmarks(filePath);
            else
                ranges = GetBookmarks(filePath, arguments[2]);

            (string prefix, string suffix) = await BookmarkMethods.AskForAffixes(dialogAssist, configuration);
            IEnumerable<ILeveledBookmark> bookmarks = BookmarkMethods.AddAffixes(ranges, prefix, suffix);

            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            CancellationToken token = cancellationSource.Token;
            ProgressDialog dialog = new ProgressDialog(string.Empty, cancellationSource)
            {
                TotalPercent = 0,
                Phase = Resources.Operations.PhaseNames.Extracting
            };

            IProgress<ProgressReport> progress = new Progress<ProgressReport>(report =>
            {
                dialog.TotalPercent = report.Percentage;
                dialog.Phase = report.CurrentPhase.GetResourceString();
            });

            Task showProgress = dialogAssist.Show(dialog);
            Task extract = manipulator.ExtractAsync(filePath, new DirectoryInfo(dir), bookmarks,
                progress, token);

            await showProgress;
        }

        private async Task ExtractDirectory(string[] arguments)
        {
            if (arguments.Length < 2 || arguments.Length > 3)
                return;

            string parentFolder = input.OpenDirectory(Resources.UserInput.Descriptions.SelectSaveFolder);
            if (parentFolder == null)
                return;

            string directoryPath = arguments[1];

            List<FileSystemInfo> createdPaths = new();
            string[] files = Directory.GetFiles(directoryPath, "*.pdf", SearchOption.AllDirectories);
            int totalAmount = files.Count() * 100;
            int currentAmount = 0;

            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            CancellationToken token = cancellationSource.Token;
            ProgressDialog dialog = new ProgressDialog(string.Empty, cancellationSource)
            {
                TotalPercent = 0,
                Phase = Resources.Operations.PhaseNames.Extracting
            };
            IProgress<ProgressReport> progress = new Progress<ProgressReport>(report =>
            {
                currentAmount += report.Percentage;
                dialog.TotalPercent = currentAmount * 100 / totalAmount;
            });

            (string prefix, string suffix) = await BookmarkMethods.AskForAffixes(dialogAssist, configuration);

            Task showProgress = dialogAssist.Show(dialog);
            Task extract = InternalExtractAll(files, parentFolder, arguments, progress, token, createdPaths,
                prefix, suffix);

            await Task.WhenAll(showProgress, extract);

            if (token.IsCancellationRequested)
            {
                foreach (FileSystemInfo info in createdPaths)
                {
                    if (info.Exists)
                    {
                        if (info is DirectoryInfo dir)
                            dir.Delete(true);
                        else
                            info.Delete();
                    }
                }
            }
        }

        private async Task InternalExtractAll(string[] files, string parentFolder, 
            string[] arguments, IProgress<ProgressReport> progress, CancellationToken token,
            List<FileSystemInfo> createdPaths, string prefix, string suffix)
        {
            foreach (string file in files)
            {
                if (token.IsCancellationRequested)
                    break;

                string dirLocation = Path.Combine(parentFolder,
                    Path.GetFileNameWithoutExtension(file) + Resources.DefaultValues.DefaultValues.UnsignedSuffix);

                IList<ILeveledBookmark> ranges;
                if (arguments.Length == 2)
                    ranges = GetBookmarks(file);
                else
                    ranges = GetBookmarks(file, arguments[2]);

                IEnumerable<ILeveledBookmark> bookmarks = BookmarkMethods.AddAffixes(ranges, prefix, suffix);

                createdPaths.AddRange(await manipulator.ExtractAsync(file, new DirectoryInfo(dirLocation), bookmarks,
                    progress, token));
            }
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
            return AddPrefixSuffix(foundBookMarks).ToList();
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

            return AddPrefixSuffix(selected).ToList();
        }

        private IEnumerable<ILeveledBookmark> AddPrefixSuffix(IEnumerable<ILeveledBookmark> bookmarks)
        {
            if (string.IsNullOrEmpty(configuration.ExtractionPrefix) &&
                string.IsNullOrEmpty(configuration.ExtractionSuffix))
            {
                return bookmarks;
            }    

            IList<ILeveledBookmark> added = new List<ILeveledBookmark>();
            foreach (ILeveledBookmark bookmark in bookmarks)
            {
                string title = string.Empty;
                if (!string.IsNullOrEmpty(configuration.ExtractionPrefix))
                {
                    title = configuration.ExtractionPrefix + " ";
                }
                title = title + bookmark.Title;
                if (!string.IsNullOrEmpty(configuration.ExtractionSuffix))
                {
                    title = title + " " + configuration.ExtractionSuffix;
                }

                added.Add(new LeveledBookmark(bookmark.Level, title, bookmark.Pages));
            }

            return added;
        }
    }
}
