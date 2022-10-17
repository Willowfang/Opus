using CX.LoggingLib;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using CX.ZipLib;
using Opus.Core.Wrappers;
using Opus.Services.Configuration;
using Opus.Services.Implementation.Data.Extraction;
using Opus.Services.Implementation.Logging;
using Opus.Services.Implementation.StaticHelpers;
using Opus.Services.Implementation.UI.Dialogs;
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
    /// Service for extracting bookmarks from files.
    /// <para>Has methods for saving into one or more pdfs or as a zip-file.</para>
    /// <para>Default implementation is included in the same namespace.</para>
    /// </summary>
    public interface IExtractionExecutor
    {
        /// <summary>
        /// Save selected bookmarks into one or more files.
        /// </summary>
        /// <param name="destination">Destination as a file or folder path (depending on
        /// whether bookmarks are saved in a single or multiple files).</param>
        /// <param name="files"></param>
        /// <returns></returns>
        public Task Save(FileSystemInfo destination, IList<FileAndBookmarkWrapper> files);
        public Task SaveAsZip(
            FileSystemInfo fileDestination,
            IList<FileAndBookmarkWrapper> files,
            FileInfo zipFile
        );
    }

    public class ExtractionExecutor : LoggingCapable<ExtractionExecutor>, IExtractionExecutor
    {
        private readonly IConfiguration configuration;
        private readonly IDialogAssist dialogAssist;
        private readonly IExtractionService extractionService;
        private readonly IAnnotationService annotationService;
        private readonly IZipService zipService;
        private bool canceled;

        public ExtractionExecutor(
            IConfiguration configuration,
            IDialogAssist dialogAssist,
            IExtractionService extractionService,
            IAnnotationService annotationService,
            IZipService zipService,
            ILogbook logbook
        ) : base(logbook)
        {
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.extractionService = extractionService;
            this.annotationService = annotationService;
            this.zipService = zipService;
        }

        public async Task SaveAsZip(
            FileSystemInfo fileDestination,
            IList<FileAndBookmarkWrapper> files,
            FileInfo zipFile
        )
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            ProgressContainer container = dialogAssist.ShowProgress(tokenSource);

            await SaveInternal(fileDestination, files, tokenSource, container);

            container.Reporting.Report(new ProgressReport(0, ProgressPhase.Converting));

            DirectoryInfo compressionDir;

            if (fileDestination is DirectoryInfo dir)
            {
                compressionDir = dir;
            }
            else
            {
                compressionDir = (fileDestination as FileInfo).Directory;
            }

            await zipService.Compress(compressionDir, zipFile);

            container.Reporting.Report(new ProgressReport(100, ProgressPhase.Finished));

            await container.Show;
        }

        public async Task Save(FileSystemInfo destination, IList<FileAndBookmarkWrapper> files)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            ProgressContainer container = dialogAssist.ShowProgress(tokenSource);

            await SaveInternal(destination, files, tokenSource, container);

            if (container.ProgressDialog.IsCanceled == false)
                container.Reporting.Report(new ProgressReport(100, ProgressPhase.Finished));

            await container.Show;
        }

        private async Task SaveInternal(
            FileSystemInfo destination,
            IList<FileAndBookmarkWrapper> files,
            CancellationTokenSource tokenSource,
            ProgressContainer container
        )
        {
            List<FileAndExtractables> products = new List<FileAndExtractables>();

            string title = await BookmarkMethods.AskForTitle(dialogAssist, configuration);

            bool destinationIsFileOrNumbered =
                destination is FileInfo || title.Contains(Resources.Placeholders.FileNames.Number);

            IEnumerable<FileAndBookmarkWrapper> renamed = BookmarkMethods.GetRenamed(
                files,
                title,
                logbook.BaseLogbook
            );

            var query = renamed.GroupBy(b => b.FilePath);

            foreach (var result in query)
            {
                if (configuration.GroupByFiles == false)
                {
                    IOrderedEnumerable<FileAndBookmarkWrapper> ordered = result.OrderBy(
                        w => w.Index
                    );
                    foreach (var item in ordered)
                    {
                        products.Add(
                            new FileAndExtractables(
                                result.Key,
                                new List<ILeveledBookmark>() { item.Bookmark }
                            )
                        );
                    }
                }
                else
                {
                    products.Add(
                        new FileAndExtractables(result.Key, result.Select(w => w.Bookmark))
                    );
                }
            }

            ExtractionOptions options = new ExtractionOptions(products, destination);

            await SelectAnnotations(options, files);

            if (canceled)
                return;

            CancellationToken token = tokenSource.Token;

            options.Progress = container.Reporting;
            options.Cancellation = token;
            options.PdfA = configuration.ExtractionConvertPdfA;

            List<string> failedConversions = new List<string>();
            options.PdfAConversionFinished += async (s, e) =>
            {
                if (e.WasFaulted == true)
                {
                    tokenSource.Cancel();
                    string content = $"{Resources.Messages.Extraction.PdfAConversionError}.";
                    MessageDialog message = new MessageDialog(
                        Resources.Labels.General.Notification,
                        content
                    );
                    await dialogAssist.Show(message);
                }
            };

            if (token.IsCancellationRequested)
            {
                logbook.Write(
                    $"Extraction was cancelled with token {token.GetHashCode()}.",
                    LogLevel.Debug
                );
                return;
            }

            logbook.Write("Extraction options: {@Options}", LogLevel.Debug, customContent: options);

            await extractionService.Extract(options);
        }

        private async Task<IList<FileAndBookmarkWrapper>> OrderBookmarks(
            IList<FileAndBookmarksStorage> files,
            bool showDialog,
            bool singleFile
        )
        {
            ExtractOrderDialog orderDialog = new ExtractOrderDialog(
                Resources.Labels.Dialogs.ExtractionOrder.Title,
                singleFile,
                configuration.GroupByFiles
            );
            foreach (FileAndBookmarksStorage file in files)
            {
                IEnumerable<ILeveledBookmark> bookmarks = file.Bookmarks
                    .Where(x => x.IsSelected)
                    .Select(y => y.Bookmark);
                bookmarks = BookmarkMethods.GetParentsOnly(bookmarks);
                foreach (ILeveledBookmark bookmark in bookmarks)
                {
                    orderDialog.Bookmarks.Add(new FileAndBookmarkWrapper(bookmark, file.FilePath));
                }
            }
            orderDialog.UpdateIndexes();

            if (showDialog)
            {
                await dialogAssist.Show(orderDialog);
                if (orderDialog.IsCanceled)
                {
                    logbook.Write(
                        $"Cancellation in {nameof(IDialog)} '{orderDialog.DialogTitle}'.",
                        LogLevel.Information
                    );
                    return null;
                }
                configuration.GroupByFiles = orderDialog.GroupByFiles;
            }

            return orderDialog.Bookmarks.ToList();
        }

        private async Task SelectAnnotations(
            ExtractionOptions options,
            IList<FileAndBookmarkWrapper> files
        )
        {
            options.Annotations = (AnnotationOption)configuration.Annotations;

            if (
                options.Annotations == AnnotationOption.Keep
                || options.Annotations == AnnotationOption.RemoveAll
            )
                return;

            HashSet<string> allTitlesSet = new HashSet<string>();

            foreach (FileAndBookmarkWrapper file in files)
            {
                if (string.IsNullOrWhiteSpace(file.FilePath))
                    continue;

                foreach (string title in await annotationService.GetTitles(file.FilePath))
                {
                    allTitlesSet.Add(title);
                }
            }

            if (allTitlesSet.Count() == 0)
                return;

            List<string> allTitlesList = allTitlesSet.ToList();
            if (allTitlesList.Contains(Environment.UserName))
            {
                allTitlesList.Remove(Environment.UserName);
                allTitlesList.Insert(0, Environment.UserName);
            }

            ExtractAnnotationsDialog annotDialog = new ExtractAnnotationsDialog(
                Resources.Labels.Dialogs.ExtractionAnnotations.Title,
                allTitlesList
            );

            await dialogAssist.Show(annotDialog);

            if (annotDialog.IsCanceled)
            {
                canceled = true;
                return;
            }

            options.AnnotationUsersToRemove = annotDialog.GetSelectedCreators();
        }
    }
}
