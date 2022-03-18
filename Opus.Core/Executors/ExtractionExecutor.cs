using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using Opus.Core.Wrappers;
using Opus.Services.Configuration;
using Opus.Services.Extensions;
using Opus.Services.Implementation.Data.Extraction;
using Opus.Services.Implementation.StaticHelpers;
using Opus.Services.Implementation.UI.Dialogs;
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
    public interface IExtractionExecutor
    {
        public Task Save(FileSystemInfo destination, IList<FileAndBookmarksStorage> files);
    }
    public class ExtractionExecutor : IExtractionExecutor
    {
        private IConfiguration configuration;
        private IDialogAssist dialogAssist;
        private IManipulator manipulator;

        public ExtractionExecutor(IConfiguration configuration, IDialogAssist dialogAssist, IManipulator manipulator)
        {
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.manipulator = manipulator;
        }

        private async Task<IList<FileAndBookmarkWrapper>> GetOrderedBookmarks(IList<FileAndBookmarksStorage> files, 
            bool showDialog, bool singleFile)
        {
            ExtractOrderDialog orderDialog = new ExtractOrderDialog(Resources.Labels.Dialogs.ExtractionOrder.Title,
                singleFile, configuration.GroupByFiles);
            foreach (FileAndBookmarksStorage file in files)
            {
                IEnumerable<ILeveledBookmark> bookmarks = file.Bookmarks.Where(x => x.IsSelected).Select(y => y.Value);
                bookmarks = BookmarkMethods.GetParentsOnly(bookmarks);
                foreach (ILeveledBookmark bookmark in bookmarks)
                {
                    orderDialog.Bookmarks.Add(new FileAndBookmarkWrapper(bookmark, file.FilePath));
                }
            }

            if (showDialog)
            {
                await dialogAssist.Show(orderDialog);
                if (orderDialog.IsCanceled) return null;
                configuration.GroupByFiles = orderDialog.GroupByFiles;
            }

            return orderDialog.Bookmarks.ToList();
        }

        public async Task Save(FileSystemInfo destination, IList<FileAndBookmarksStorage> files)
        {
            List<FileAndExtractables> products = new List<FileAndExtractables>();

            string title = await BookmarkMethods.AskForTitle(dialogAssist, configuration);

            IList<FileAndBookmarkWrapper> orderedBookmarks = await GetOrderedBookmarks(files, destination is FileInfo
                || title.Contains(Resources.DefaultValues.DefaultValues.Number), destination is FileInfo);

            if (orderedBookmarks == null) return;

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var result = ShowProgress(tokenSource);

            foreach (FileAndBookmarksStorage storage in files)
            {
                IEnumerable<ILeveledBookmark> bookmarks = storage.Bookmarks.Where(x => x.IsSelected).Select(y => y.Value);

                bookmarks = BookmarkMethods.GetParentsOnly(bookmarks);

                IEnumerable<FileAndBookmarkWrapper> renamedIndexed = BookmarkMethods.GetRenamedAndIndexed(bookmarks, orderedBookmarks, title, storage.FilePath);

                if (configuration.GroupByFiles == false)
                {
                    renamedIndexed = renamedIndexed.OrderBy(w => w.Index);
                    foreach (FileAndBookmarkWrapper wrap in renamedIndexed)
                    {
                        products.Add(new FileAndExtractables(storage.FilePath, new List<ILeveledBookmark>() { wrap.Bookmark }));
                    }
                }
                else
                {
                    products.Add(new FileAndExtractables(storage.FilePath, renamedIndexed.Select(w => w.Bookmark)));
                }
            }

            ExtractionOptions options = new ExtractionOptions(products, destination);

            options.Progress = result.progress;
            options.Cancellation = token;
            options.PdfA = configuration.ExtractionConvertPdfA;
            options.Annotations = (AnnotationOption)configuration.Annotations;

            List<string> failedConversions = new List<string>();
            options.PdfAConversionFinished += async (s, e) =>
            {
                if (e.WasFaulted == true)
                {
                    tokenSource.Cancel();
                    string content = $"{Resources.Messages.Extraction.PdfAConversionError}.";
                    MessageDialog message = new MessageDialog(Resources.Labels.General.Notification, content);
                    await dialogAssist.Show(message);
                }
            };

            if (token.IsCancellationRequested) return;

            Task extract = manipulator.ExtractAsync(options);

            await result.dialog;
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
