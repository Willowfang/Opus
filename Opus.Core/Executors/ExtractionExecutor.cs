﻿using CX.LoggingLib;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using Opus.Core.Wrappers;
using Opus.ExtensionMethods;
using Opus.Services.Configuration;
using Opus.Services.Extensions;
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
    public interface IExtractionExecutor
    {
        public Task Save(FileSystemInfo destination, IList<FileAndBookmarksStorage> files);
    }
    public class ExtractionExecutor : LoggingCapable<ExtractionExecutor>, IExtractionExecutor
    {
        private IConfiguration configuration;
        private IDialogAssist dialogAssist;
        private IExtractionService extractionService;
        private IAnnotationService annotationService;
        private bool canceled;

        public ExtractionExecutor(
            IConfiguration configuration, 
            IDialogAssist dialogAssist, 
            IExtractionService extractionService,
            IAnnotationService annotationService,
            ILogbook logbook) : base(logbook)
        {
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.extractionService = extractionService;
            this.annotationService = annotationService;
        }

        public async Task Save(FileSystemInfo destination, IList<FileAndBookmarksStorage> files)
        {
            List<FileAndExtractables> products = new List<FileAndExtractables>();

            string title = await BookmarkMethods.AskForTitle(dialogAssist, configuration);

            bool destinationIsFileOrNumbered = destination is FileInfo ||
                title.Contains(Resources.Placeholders.FileNames.Number);

            IList<FileAndBookmarkWrapper> orderedBookmarks = await OrderBookmarks(files, destinationIsFileOrNumbered,
                destination is FileInfo);

            if (orderedBookmarks == null) return;

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

            await SelectAnnotations(options, files);

            if (canceled) return;

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            ProgressContainer container = dialogAssist.ShowProgress(tokenSource);

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
                    MessageDialog message = new MessageDialog(Resources.Labels.General.Notification, content);
                    await dialogAssist.Show(message);
                }
            };

            if (token.IsCancellationRequested)
            {
                logbook.Write($"Extraction was cancelled with token {token.GetHashCode()}.", LogLevel.Debug);
                return;
            }

            logbook.Write("Extraction options: {@Options}", LogLevel.Debug, customContent: options);

            Task extract = extractionService.Extract(options);

            await Task.WhenAll(extract, container.Show);
        }

        private async Task<IList<FileAndBookmarkWrapper>> OrderBookmarks(IList<FileAndBookmarksStorage> files,
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
                if (orderDialog.IsCanceled)
                {
                    logbook.Write($"Cancellation in {nameof(IDialog)} '{orderDialog.DialogTitle}'.", LogLevel.Information);
                    return null;
                }
                configuration.GroupByFiles = orderDialog.GroupByFiles;
            }

            return orderDialog.Bookmarks.ToList();
        }

        private async Task SelectAnnotations(ExtractionOptions options, IList<FileAndBookmarksStorage> files)
        {
            options.Annotations = (AnnotationOption)configuration.Annotations;

            if (options.Annotations == AnnotationOption.Keep ||
                options.Annotations == AnnotationOption.RemoveAll)
                return;

            HashSet<string> allTitlesSet = new HashSet<string>();

            foreach (FileAndBookmarksStorage file in files)
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
                Resources.Labels.Dialogs.ExtractionAnnotations.Title, allTitlesList);

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