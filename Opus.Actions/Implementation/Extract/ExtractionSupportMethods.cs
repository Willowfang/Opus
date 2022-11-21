using Opus.Actions.Services.Extract;
using Opus.Common.Logging;
using Opus.Common.Dialogs;
using Opus.Common.Services.Input;
using WF.LoggingLib;
using Opus.Common.Services.Dialogs;
using Opus.Common.Extensions;
using Opus.Common.Helpers;
using Opus.Events.Data;
using Opus.Common.Wrappers;
using WF.PdfLib.Common;
using Opus.Events;
using Prism.Events;
using Opus.Common.Services.Configuration;
using WF.PdfLib.Services;
using WF.ZipLib;
using WF.PdfLib.Services.Data;
using Opus.Common.Progress;
using System.Security.Cryptography.Pkcs;

namespace Opus.Actions.Implementation.Extract
{
    /// <summary>
    /// Implementation for <see cref="IExtractionSupportMethods"/>.
    /// </summary>
    public class ExtractionSupportMethods : LoggingCapable<ExtractionSupportMethods>, IExtractionSupportMethods
    {
        private readonly IPathSelection pathSelection;
        private readonly IDialogAssist dialogAssist;
        private readonly IExtractionSupportProperties properties;
        private readonly IEventAggregator eventAggregator;
        private readonly IConfiguration configuration;
        private readonly IExtractionService extraction;
        private readonly IAnnotationService annotations;
        private readonly IZipService compression;

        /// <summary>
        /// Create a new implementation instance.
        /// </summary>
        /// <param name="pathSelection">User path selection service.</param>
        /// <param name="dialogAssist">Dialog showing service.</param>
        /// <param name="properties">Extraction support action properties service.</param>
        /// <param name="eventAggregator">Event service.</param>
        /// <param name="configuration">Application configuration service.</param>
        /// <param name="extraction">Service for performing extraction.</param>
        /// <param name="annotations">Annotation manipulation service.</param>
        /// <param name="compression">Service for compressing files.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionSupportMethods(
            IPathSelection pathSelection,
            IDialogAssist dialogAssist,
            IExtractionSupportProperties properties,
            IEventAggregator eventAggregator,
            IConfiguration configuration,
            IExtractionService extraction,
            IAnnotationService annotations,
            IZipService compression,
            ILogbook logbook) : base(logbook)
        {
            this.pathSelection = pathSelection;
            this.dialogAssist = dialogAssist;
            this.properties = properties;
            this.eventAggregator = eventAggregator;
            this.configuration = configuration;
            this.extraction = extraction;
            this.annotations = annotations;
            this.compression = compression;
        }

        #region Save
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task Save(FileSystemInfo destination, IList<FileAndBookmarkWrapper> files)
        {
            logbook.Write($"Saving file or files.", LogLevel.Debug);

            ProgressTracker progress = new ProgressTracker(SaveGetGrandTotal(destination, files), dialogAssist);

            IExtractionWorker worker = SaveGetExtractionWorker(progress, destination);

            // Get bookmarks renamed by template.

            IEnumerable<FileAndBookmarkWrapper> renamed = await SaveGetRenamedBookmarks(files);

            // Get products. They will be grouped by file, if said setting has been
            // selected by the user.

            List<FileAndExtractables> products = SaveGetProducts(renamed);

            // Get extraction options and ask the user for annotations to remove, if annotation removal
            // by user has been selected.

            ExtractionOptions? options = await SaveGetOptions(
                products, 
                destination, 
                files,
                progress);

            if (options == null)
            {
                logbook.Write($"Extraction options were null.", LogLevel.Warning);

                return;
            }

            // Extract the bookmarks

            progress.Update(0, ProgressPhase.Extracting);

            await worker.Extract(options);

            // Compress into a zip-file, if such setting is on.

            FileSystemInfo openDestination = destination;

            if (configuration.ExtractionCreateZip)
            {
                openDestination = await SaveCreateZip(destination, progress, openDestination);
            }

            // Show progress dialog until cancelled or user closes after success.

            if (progress.IsCanceled == false)
                progress.SetToComplete();

            await progress.Show;

            if (configuration.ExtractionOpenDestinationAfterComplete
                && progress.IsCanceled == false) openDestination.OpenPdfOrDirectory();
        }

        private async Task<FileSystemInfo> SaveCreateZip(
            FileSystemInfo destination,
            ProgressTracker progress,
            FileSystemInfo initialOpenDestination)
        {
            logbook.Write($"Creating zip file.", LogLevel.Debug);

            FileSystemInfo returnOpenDestination = initialOpenDestination;

            FileInfo? zipFile = await Task.Run(() => GetSaveLocation(properties, FileType.Zip));

            if (zipFile == null)
            {
                logbook.Write($"Selected file path was null.", LogLevel.Warning);

                progress.Cancel();
            }
            else
            {
                returnOpenDestination = zipFile;

                await SaveCompress(destination, progress, zipFile);

                logbook.Write($"Zip file created.", LogLevel.Debug);
            }

            return returnOpenDestination;
        }

        private int SaveGetGrandTotal(FileSystemInfo destination, IList<FileAndBookmarkWrapper> files)
        {
            logbook.Write($"Retrieving grand total for file saving.", LogLevel.Debug);

            int grandTotal = 0;

            if (destination is FileInfo)
            {
                HashSet<string> paths = new HashSet<string>(files.Select(x => x.FilePath));

                grandTotal = paths.Count * 2;
            }
            else
            {
                grandTotal = files.Count;
            }

            if (configuration.ExtractionConvertPdfA) grandTotal++;

            logbook.Write($"Grand total of {grandTotal} retrieved.", LogLevel.Debug);

            return grandTotal;
        }

        private IExtractionWorker SaveGetExtractionWorker(ProgressTracker tracker, FileSystemInfo destination)
        {
            logbook.Write($"Creating extraction worker.", LogLevel.Debug);

            IExtractionWorker worker = extraction.CreateWorker();

            logbook.Write($"Assigning event handlers to worker events.", LogLevel.Debug);

            SaveAssignWorkerBookmarkEvent(worker, tracker, destination);

            if (destination is FileInfo) SaveAssingWorkerMergeEvent(worker, tracker);

            if (configuration.ExtractionConvertPdfA) SaveAssignWorkerPdfAEvent(worker, tracker);

            logbook.Write($"Worker event handlers assigned.", LogLevel.Debug);

            logbook.Write($"Extraction worker created.", LogLevel.Debug);

            return worker;
        }

        private void SaveAssignWorkerBookmarkEvent(
            IExtractionWorker worker, 
            ProgressTracker tracker,
            FileSystemInfo destination)
        {
            worker.BookmarkOrFileExtracted += (sender, args) =>
            {
                if (args.IsDone)
                {
                    if (destination is FileInfo)
                    {
                        tracker.Update(1, ProgressPhase.Merging);
                    }
                    else if (configuration.ExtractionConvertPdfA)
                    {
                        tracker.Update(1, ProgressPhase.Converting);
                        tracker.SetPercentage(0);
                    }
                    else if (configuration.ExtractionCreateZip)
                    {
                        tracker.SetToUndefined();
                    }
                    else
                    {
                        tracker.SetToComplete();
                    }
                }
                else
                {
                    tracker.Update(1);
                }
            };
        }

        private void SaveAssingWorkerMergeEvent(
            IExtractionWorker worker,
            ProgressTracker tracker)
        {
            worker.FileMerged += (sender, args) =>
            {
                if (args.IsDone)
                {
                    if (configuration.ExtractionConvertPdfA)
                    {
                        tracker.Update(1, ProgressPhase.Converting);
                        tracker.SetPercentage(0);
                    }
                    else if (configuration.ExtractionCreateZip)
                    {
                        tracker.SetToUndefined();
                    }
                    else
                    {
                        tracker.SetToComplete();
                    }
                }
                else
                {
                    tracker.Update(1);
                }
            };
        }

        private void SaveAssignWorkerPdfAEvent(
            IExtractionWorker worker,
            ProgressTracker tracker)
        {
            worker.FilesConverted += async (sender, args) =>
            {
                if (configuration.ExtractionCreateZip)
                {
                    tracker.SetToUndefined();
                }
                else
                {
                    if (args.WasFaulted)
                    {
                        MessageDialog dialog = new MessageDialog(
                            Resources.Labels.General.Error,
                            Resources.Messages.Extraction.PdfAConversionError);

                        await dialogAssist.Show(dialog);
                    }

                    tracker.SetToComplete();
                }
            };
        }

        private async Task<IEnumerable<FileAndBookmarkWrapper>> SaveGetRenamedBookmarks(
            IList<FileAndBookmarkWrapper> files)
        {
            // If asking for title has been chosen, ask the user for title template. Otherwise,
            // use the given template.

            logbook.Write($"Retrieving renamed bookmarks.", LogLevel.Debug);

            string title = await BookmarkMethods.AskForTitle(dialogAssist, configuration);

            // Return bookmarks renamed according to the template.

            return BookmarkMethods.GetRenamed(
                files,
                title,
                logbook.BaseLogbook
            );
        }

        private List<FileAndExtractables> SaveGetProducts(IEnumerable<FileAndBookmarkWrapper> renamed)
        {
            logbook.Write($"Retrieving grouped products for extraction.", LogLevel.Debug);

            List<FileAndExtractables> products = new List<FileAndExtractables>();

            var query = renamed.GroupBy(b => b.FilePath);

            foreach (var result in query)
            {
                if (configuration.GroupByFiles == false)
                {
                    SaveAddProductsUngrouped(products, result);
                }
                else
                {
                    products.Add(
                        new FileAndExtractables(result.Key, result.Select(w => w.Bookmark))
                    );
                }
            }

            logbook.Write($"Products retrieved.", LogLevel.Debug);

            return products;
        }

        private void SaveAddProductsUngrouped(
            List<FileAndExtractables> products,
            IGrouping<string, FileAndBookmarkWrapper> result)
        {
            IOrderedEnumerable<FileAndBookmarkWrapper> ordered = result.OrderBy(w => w.Index);

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

        private async Task<ExtractionOptions?> SaveGetOptions(
            List<FileAndExtractables> products,
            FileSystemInfo destination,
            IList<FileAndBookmarkWrapper> files,
            ProgressTracker tracker)
        {
            logbook.Write($"Retrieving extraction options.", LogLevel.Debug);

            ExtractionOptions options = new ExtractionOptions(products, destination)
            {
                Annotations = (AnnotationOption)configuration.Annotations
            };

            IEnumerable<string>? selectedAnnotationUsers = await SelectAnnotations(options, files);

            if (selectedAnnotationUsers == null)
            {
                return null;
            }

            options.AnnotationUsersToRemove = selectedAnnotationUsers;

            CancellationToken token = tracker.Token;

            options.Cancellation = token;

            options.ConvertToPdfA = configuration.ExtractionConvertPdfA;

            if (token.IsCancellationRequested)
            {
                logbook.Write($"Extraction was cancelled.", LogLevel.Information);

                return null;
            }

            logbook.Write($"Options retrieved.", LogLevel.Debug);

            return options;
        }

        private async Task SaveCompress(
            FileSystemInfo destination, 
            ProgressTracker tracker, 
            FileInfo zipFile)
        {
            logbook.Write($"Compressing files.", LogLevel.Debug);

            tracker.Update(0, ProgressPhase.Converting);

            // Compress the files into a zip-file.

            DirectoryInfo compressionDir;

            if (destination is DirectoryInfo dir)
            {
                compressionDir = dir;
            }
            else
            {
                compressionDir = (destination as FileInfo)!.Directory!;
            }

            await compression.Compress(compressionDir, zipFile);

            compressionDir.Delete(true);

            logbook.Write($"Files compressed, original directory deleted.", LogLevel.Debug);
        }

        private async Task<FileSystemInfo?> SaveCompressGetDestination(bool saveSingular)
        {
            logbook.Write($"Retrieving destination for compressed file.", LogLevel.Debug);

            // The destination path

            FileSystemInfo destination;

            // Create a temporary, randomly named directory in the TEMP directory to hold the extracted files
            // before they are compressed.

            DirectoryInfo tempDir = SaveAsZipCreateTempDirectory();

            // Save bookmarks into a single file.

            if (saveSingular)
            {
                FileInfo? singular = await GetSingularZipPath(tempDir, properties);

                if (singular == null) return null;

                destination = singular;
            }

            // Save bookmarks in separate files (store them in the temp directory before compressing).

            else
            {
                destination = tempDir;
            }

            logbook.Write($"Compression file destination retrieved.", LogLevel.Debug);

            return destination;
        }
        #endregion

        #region Select annotations private
        private async Task<IEnumerable<string>?> SelectAnnotations(
            ExtractionOptions options,
            IList<FileAndBookmarkWrapper> files)
        {
            logbook.Write($"Retrieving info on annotations to remove.", LogLevel.Debug);

            // If all annotations are kept or removed, do nothing (they will be handled
            // accordingly).

            if (options.Annotations == AnnotationOption.Keep
                || options.Annotations == AnnotationOption.RemoveAll)
            {
                logbook.Write($"Annotation removal info retrieved.", LogLevel.Debug);

                return new List<string>();
            }

            List<string> allTitles = await SelectAnnotationsGetAllTitles(files);

            // No annotations (or no annotations with titles) in the document, so
            // nothing to ask the user about. Return without showing a dialog.

            if (allTitles.Count == 0)
            {
                logbook.Write($"Annotation removal info retrieved.", LogLevel.Debug);

                return new List<string>();
            }

            SelectAnnotationsMoveUserTitleToTop(allTitles);

            ExtractAnnotationsDialog dialog = await SelectAnnotationsShowDialog(allTitles);

            if (dialog.IsCanceled)
            {
                logbook.Write($"Action was cancelled.", LogLevel.Information);

                return null;
            }

            logbook.Write($"Annotation removal info retrieved.", LogLevel.Debug);

            return dialog.GetSelectedCreators();
        }

        private async Task<List<string>> SelectAnnotationsGetAllTitles(IList<FileAndBookmarkWrapper> files)
        {
            logbook.Write($"Retrieving all annotation titles from documents.", LogLevel.Debug);

            HashSet<string> allTitlesSet = new HashSet<string>();

            foreach (FileAndBookmarkWrapper file in files)
            {
                if (string.IsNullOrWhiteSpace(file.FilePath))
                    continue;

                foreach (string title in await annotations.GetTitles(file.FilePath))
                {
                    allTitlesSet.Add(title);
                }
            }

            logbook.Write($"Annotation titles retrieved.", LogLevel.Debug);

            return allTitlesSet.ToList();
        }

        private void SelectAnnotationsMoveUserTitleToTop(List<string> allTitles)
        {
            if (allTitles.Contains(Environment.UserName))
            {
                allTitles.Remove(Environment.UserName);
                allTitles.Insert(0, Environment.UserName);
            }
        }

        private async Task<ExtractAnnotationsDialog> SelectAnnotationsShowDialog(List<string> allTitles)
        {
            ExtractAnnotationsDialog dialog = new ExtractAnnotationsDialog(
                Resources.Labels.Dialogs.ExtractionAnnotations.Title,
                allTitles
            );

            await dialogAssist.Show(dialog);

            return dialog;
        }
        #endregion

        #region Entry update
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="properties"></param>
        public void UpdateEntries(IExtractionSupportProperties properties)
        {
            logbook.Write($"Updating indexes for entries.", LogLevel.Debug);

            for (int i = 0; i < properties.Bookmarks.Count; i++)
            {
                properties.Bookmarks[i].Index = i + 1;
            }

            properties.RaiseChanged(nameof(properties.CollectionHasActualBookmarks));

            logbook.Write($"Entry indexes updated.", LogLevel.Debug);
        }
        #endregion

        #region Execute add external
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteAddExternal()
        {
            // Insert an empty placeholder bookmark at the end of the collection

            logbook.Write($"Adding marker for external file.", LogLevel.Debug);

            properties.Bookmarks.Add(
                BookmarkMethods.GetPlaceHolderBookmarkWrapper(
                    Resources.Labels.Dialogs.ExtractionOrder.ExternalFile.ToUpper(),
                    Resources.Labels.Dialogs.ExtractionOrder.ExternalFile.ToUpper(),
                    properties.Bookmarks.Count + 1
                )
            );

            logbook.Write($"External file marker added.", LogLevel.Debug);
        }
        #endregion

        #region Execute edit
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task ExecuteEdit()
        {
            logbook.Write($"Editing bookmark.", LogLevel.Information);

            // Show a dialog with editing options to the user. Prefill with relevant info.

            BookmarkDialog? dialog = await EditShowBookmarkDialog(properties);

            // The user canceled, just return

            if (dialog == null || dialog.IsCanceled)
            {
                logbook.Write($"Edit was cancelled or selected item was null.", LogLevel.Information);

                return;
            }
            else
            {
                EditCreateNewAndReplaceSelected(properties, dialog);
            }

            logbook.Write($"Bookmark edited.", LogLevel.Information);
        }

        private async Task<BookmarkDialog?> EditShowBookmarkDialog(IExtractionSupportProperties properties)
        {
            if (properties.Bookmarks.SelectedItem == null) return null;

            BookmarkDialog dialog = new BookmarkDialog(
                Resources.Labels.Dialogs.Bookmark.Edit,
                properties.Bookmarks.SelectedItem.Bookmark.Title)
            {
                StartPage = properties.Bookmarks.SelectedItem.Bookmark.StartPage,
                EndPage = properties.Bookmarks.SelectedItem.Bookmark.EndPage
            };

            await dialogAssist.Show(dialog);

            return dialog;
        }

        private void EditCreateNewAndReplaceSelected(
            IExtractionSupportProperties properties,
            BookmarkDialog dialog)
        {
            logbook.Write($"Replacing original bookmark.", LogLevel.Debug);

            if (properties.Bookmarks.SelectedItem == null)
            {
                logbook.Write($"Selected bookmark was null.", LogLevel.Warning);

                return;
            }

            LeveledBookmark innerMark = new LeveledBookmark(
                properties.Bookmarks.SelectedItem.Bookmark.Level,
                dialog.Title,
                dialog.StartPage,
                dialog.EndPage - dialog.StartPage + 1);

            FileAndBookmarkWrapper updated = new FileAndBookmarkWrapper(
                innerMark,
                properties.Bookmarks.SelectedItem.FilePath,
                properties.Bookmarks.SelectedItem.Index,
                properties.Bookmarks.SelectedItem.Id
            );

            int index = properties.Bookmarks.IndexOf(properties.Bookmarks.SelectedItem);

            properties.Bookmarks.RemoveSelected();

            properties.Bookmarks.Insert(index, updated);

            logbook.Write($"Bookmark replaced.", LogLevel.Debug);
        }
        #endregion

        #region Execute delete
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ExecuteDelete()
        {
            logbook.Write($"Deleting bookmark.", LogLevel.Debug);

            if (properties.Bookmarks.SelectedItem == null)
            {
                logbook.Write($"Selected bookmark was null.", LogLevel.Warning);

                return;
            }

            // Publish a bookmark deletion event with relevant info (Guid of the bookmark). Will
            // be picked up at Extraction ViewModel, where a bookmark with the same Guid will be made visible.

            eventAggregator
                .GetEvent<BookmarkDeselectedEvent>()
                .Publish(properties.Bookmarks.SelectedItem.Id);

            // Remove selected bookmark from the collection and if - after this - the collection contains no
            // other instances besides placeholders, clear it (no need to preserve just placeholders).

            properties.Bookmarks.RemoveSelected();

            if (properties.Bookmarks.All(b => b.Bookmark.Pages.Count == 0))
            {
                properties.Bookmarks.Clear();
            }

            logbook.Write($"Bookmark deleted.", LogLevel.Debug);
        }
        #endregion

        #region Execute save file
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task ExecuteSaveFile()
        {
            logbook.Write($"Extracting to one file.", LogLevel.Information);

            FileSystemInfo? destination;

            // Saving into a zip-file requested

            if (configuration.ExtractionCreateZip)
            {
                destination = await SaveCompressGetDestination(true);
            } 
            else
            {
                // Ask the user for save path for the file.

                destination = GetSaveLocation(properties, FileType.PDF);
            }

            if (destination == null)
            {
                logbook.Write($"Given destination was null.", LogLevel.Warning);

                return;
            }

            // Execute extraction

            await Save(destination, properties.Bookmarks.Extractables());

            logbook.Write($"Extraction completed.", LogLevel.Information);
        }
        #endregion

        #region Execute save separate
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public async Task ExecuteSaveSeparate()
        {
            logbook.Write($"Extracting to multiple files.", LogLevel.Information);

            FileSystemInfo? destination;

            // Saving into a zip-file requested.

            if (configuration.ExtractionCreateZip)
            {
                destination = await SaveCompressGetDestination(false);
            }
            else
            {
                // Ask the user for a save path

                string? path = pathSelection.OpenDirectory(
                    Resources.UserInput.Descriptions.SelectSaveFolder
                );

                if (path == null)
                    return;

                destination = new DirectoryInfo(path);
            }

            if (destination == null)
            {
                logbook.Write($"Given destination was null.", LogLevel.Warning);

                return;
            }

            // Execute extraction

            await Save(destination, properties.Bookmarks.Extractables());

            logbook.Write($"Extraction completed.", LogLevel.Information);
        }
        #endregion

        #region Common private
        private void OpenPathForViewing(FileSystemInfo extractionDestination)
        {
            logbook.Write($"Opening file or folder for viewing.", LogLevel.Debug);

            FileSystemInfo destination = extractionDestination;

            if (configuration.ExtractionCreateZip
                && destination is FileInfo destinationFile
                && destinationFile.Directory != null)
            {
                destination = destinationFile.Directory;
            }

            if (destination is FileInfo file)
            {
                new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(
                    file.FullName)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
            else if (destination is DirectoryInfo directory)
            {
                new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(
                    @directory.FullName)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }

            logbook.Write($"File or folder opened externally.", LogLevel.Debug);
        }
        
        private async Task<FileInfo?> GetSingularZipPath(
            DirectoryInfo tempDir,
            IExtractionSupportProperties properties)
        {
            logbook.Write($"Retrieving path for a single file when compressing.", LogLevel.Debug);

            // Ask the user for the name of the zip-file and name of the contained file.

            ExtractSettingsDialog fileNameDialog = new ExtractSettingsDialog(
                Resources.Labels.Dialogs.ExtractionOptions.ZipDialogTitle,
                true,
                Resources.Labels.Dialogs.ExtractionOptions.ZipName,
                Resources.Labels.Dialogs.ExtractionOptions.ZipNameHelper,
                false
            );

            await dialogAssist.Show(fileNameDialog);

            // User has cancelled, just return.

            if (fileNameDialog.IsCanceled)
            {
                logbook.Write($"Path retrieval was cancelled.", LogLevel.Information);

                return null;
            }

            // Get user entered filename for the zip and replace unallowed characters in the filename.

            string userPath = fileNameDialog.Title ?? properties.Bookmarks[0].Bookmark.Title;

            string fileName =
                Path.GetFileNameWithoutExtension(userPath.ReplaceIllegal()) + ".pdf";
            string fullPath = Path.Combine(tempDir.FullName, fileName);

            // Assign full path to destination path.

            logbook.Write($"Path retrieved.", LogLevel.Debug);

            return new FileInfo(fullPath);
        }

        /// <summary>
        /// Get a save path from the user.
        /// </summary>
        /// <param name="properties">Extraction support properties.</param>
        /// <param name="fileType">Type of the file to save.</param>
        /// <returns>Selected path as FileInfo. Null if canceled or empty path.</returns>
        private FileInfo? GetSaveLocation(IExtractionSupportProperties properties, FileType fileType)
        {
            logbook.Write($"Retrieving file save location.", LogLevel.Debug);

            string? firstBookmarkDirPath = Path.GetDirectoryName(properties.Bookmarks[0].FilePath);

            DirectoryInfo startDir = string.IsNullOrEmpty(firstBookmarkDirPath)
                ? new DirectoryInfo(Path.GetTempPath())
                : new DirectoryInfo(firstBookmarkDirPath);

            string? pathOfFile = pathSelection.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                fileType,
                startDir
            );

            if (string.IsNullOrEmpty(pathOfFile))
            {
                logbook.Write($"Selected path was null or empty.", LogLevel.Warning);

                return null;
            }

            return new FileInfo(pathOfFile);
        }

        private DirectoryInfo SaveAsZipCreateTempDirectory()
        {
            logbook.Write($"Creating temporary directory for compression.", LogLevel.Debug);

            DirectoryInfo tempDir = new DirectoryInfo(
                Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
            );

            tempDir.Create();

            logbook.Write($"Temporary directory created at {tempDir.FullName}.", LogLevel.Debug);

            return tempDir;
        }

        #endregion
    }
}
