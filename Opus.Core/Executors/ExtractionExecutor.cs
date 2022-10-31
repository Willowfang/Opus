using WF.LoggingLib;
using WF.PdfLib.Common;
using WF.PdfLib.Services;
using WF.PdfLib.Services.Data;
using WF.ZipLib;
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
        /// <returns>An awaitable task.</returns>
        public Task Save(FileSystemInfo destination, IList<FileAndBookmarkWrapper> files);

        /// <summary>
        /// Save selected bookmarks and compress them into a zip-file.
        /// </summary>
        /// <param name="fileDestination">Destination for the files to compress.</param>
        /// <param name="files">Bookmarks to extract.</param>
        /// <param name="zipFile">Location of the final zip-file.</param>
        /// <returns>An awaitable task.</returns>
        public Task SaveAsZip(
            FileSystemInfo fileDestination,
            IList<FileAndBookmarkWrapper> files,
            FileInfo zipFile
        );
    }

    /// <summary>
    /// <see cref="IExtractionExecutor"/> default implementation.
    /// <para>
    /// Has logging capability.
    /// </para>
    /// </summary>
    public class ExtractionExecutor : LoggingCapable<ExtractionExecutor>, IExtractionExecutor
    {
        // DI services
        private readonly IConfiguration configuration;
        private readonly IDialogAssist dialogAssist;
        private readonly IExtractionService extractionService;
        private readonly IAnnotationService annotationService;
        private readonly IZipService zipService;

        // Cancellation state
        private bool canceled;

        /// <summary>
        /// Create an extraction executor.
        /// </summary>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="extractionService">Service containing smaller services for extracting bookmarks.</param>
        /// <param name="annotationService">Service for dealing with pdf annotations.</param>
        /// <param name="zipService">Service for compressing files into a zip-file.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionExecutor(
            IConfiguration configuration,
            IDialogAssist dialogAssist,
            IExtractionService extractionService,
            IAnnotationService annotationService,
            IZipService zipService,
            ILogbook logbook
        ) : base(logbook)
        {
            // Assign DI services
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.extractionService = extractionService;
            this.annotationService = annotationService;
            this.zipService = zipService;
        }

        /// <summary>
        /// Extract bookmarks and compress them into a zip-file.
        /// <para>
        /// Implements <see cref="IExtractionExecutor.SaveAsZip(FileSystemInfo, IList{FileAndBookmarkWrapper}, FileInfo)"/>.
        /// </para>
        /// </summary>
        /// <param name="fileDestination">Destination path for the extracted bookmarks.</param>
        /// <param name="files">Bookmarks to extract.</param>
        /// <param name="zipFile">Filepath of the zip to compress into.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SaveAsZip(
            FileSystemInfo fileDestination,
            IList<FileAndBookmarkWrapper> files,
            FileInfo zipFile
        )
        {
            // Save the bookmarks as files in the given directory.

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            ProgressContainer container = dialogAssist.ShowProgress(tokenSource);

            await SaveInternal(fileDestination, files, tokenSource, container);

            container.Reporting.Report(new ProgressReport(0, ProgressPhase.Converting));

            // Compress the files into a zip-file.

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

        /// <summary>
        /// Extract bookmarks into one or more files.
        /// <para>
        /// Implements <see cref="IExtractionExecutor.Save(FileSystemInfo, IList{FileAndBookmarkWrapper})"/>.
        /// </para>
        /// </summary>
        /// <param name="destination">Destination file or folder.</param>
        /// <param name="files">Boomarks to extract.</param>
        /// <returns></returns>
        public async Task Save(FileSystemInfo destination, IList<FileAndBookmarkWrapper> files)
        {
            // Extract bookmarks.

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            ProgressContainer container = dialogAssist.ShowProgress(tokenSource);

            await SaveInternal(destination, files, tokenSource, container);

            // Show progress dialog until cancelled or user closes after success.

            if (container.ProgressDialog.IsCanceled == false)
                container.Reporting.Report(new ProgressReport(100, ProgressPhase.Finished));

            await container.Show;
        }

        /// <summary>
        /// Common saving functionality.
        /// </summary>
        /// <param name="destination">Destination of the bookmarks.</param>
        /// <param name="files">Bookmarks to extract.</param>
        /// <param name="tokenSource">Cancellation token.</param>
        /// <param name="container">Container for displaying progress.</param>
        /// <returns>An awaitable task.</returns>
        private async Task SaveInternal(
            FileSystemInfo destination,
            IList<FileAndBookmarkWrapper> files,
            CancellationTokenSource tokenSource,
            ProgressContainer container
        )
        {
            // If asking for title has been chosen, ask the user for title template. Otherwise,
            // use the given template.

            List<FileAndExtractables> products = new List<FileAndExtractables>();

            string title = await BookmarkMethods.AskForTitle(dialogAssist, configuration);

            bool destinationIsFileOrNumbered =
                destination is FileInfo || title.Contains(Resources.Placeholders.FileNames.Number);

            // Return bookmarks renamed according to the template.

            IEnumerable<FileAndBookmarkWrapper> renamed = BookmarkMethods.GetRenamed(
                files,
                title,
                logbook.BaseLogbook
            );

            // Group bookmarks by files and extract them per file.

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

            // If some annotation have been chosen as removables, either ask the user about them
            // or set those annotations as items to remove.

            await SelectAnnotations(options, files);

            if (canceled)
                return;

            CancellationToken token = tokenSource.Token;

            options.Progress = container.Reporting;
            options.Cancellation = token;
            options.PdfA = configuration.ExtractionConvertPdfA;

            // If pdf/a -conversion has been chosen, display error messages for the
            // conversions that have failed.

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

            // Extract the bookmarks

            logbook.Write("Extraction options: {@Options}", LogLevel.Debug, customContent: options);

            await extractionService.Extract(options);
        }

        /// <summary>
        /// DEPRECATED. Set bookmark order in a dialog.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="showDialog"></param>
        /// <param name="singleFile"></param>
        /// <returns></returns>
        private async Task<IList<FileAndBookmarkWrapper>> BookmarkOrderDialog(
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

        /// <summary>
        /// Select annotations to remove.
        /// </summary>
        /// <param name="options">Extraction options to apply.</param>
        /// <param name="files">Bookmarks.</param>
        /// <returns>An awaitable task.</returns>
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
