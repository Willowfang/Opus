using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WF.PdfLib.Common;
using WF.PdfLib.Services;
using WF.PdfLib.Services.Data;
using Opus.Services.Data;
using Opus.Services.Data.Composition;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Input;
using Opus.Services.UI;
using Opus.Services.Extensions;
using WF.LoggingLib;

namespace Opus.Services.Implementation.Data.Composition
{
    /// <summary>
    /// Default implementation for <see cref="IComposer"/> service.
    /// </summary>
    public class Composer : IComposer
    {
        #region DI services
        private IDialogAssist dialogAssist;
        private IPathSelection input;
        private ILogbook logbook;
        private IMergingService mergingService;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new Composer service implementation instance.
        /// </summary>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="input">Service for acquiring user path input.</param>
        /// <param name="mergingService">Service for dealing with merging files.</param>
        /// <param name="logbook">Logging service.</param>
        public Composer(
            IDialogAssist dialogAssist,
            IPathSelection input,
            IMergingService mergingService,
            ILogbook logbook
        )
        {
            // Assign DI services

            this.dialogAssist = dialogAssist;
            this.input = input;
            this.logbook = logbook;
            this.mergingService = mergingService;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Compose the document from a directory of files.
        /// </summary>
        /// <param name="directory">Directory to search the files in.</param>
        /// <param name="compositionProfile">Profile on which the composition is based.</param>
        /// <param name="deleteConverted">If true, delete converted files after composition has finished.</param>
        /// <param name="searchSubDirectories">If true, search subdirectories for files matching search criteria.</param>
        /// <returns>An awaitable task.</returns>
        public async Task Compose(
            string directory,
            ICompositionProfile compositionProfile,
            bool deleteConverted,
            bool searchSubDirectories
        )
        {
            // Create a worker instance to deal with composing the document.

            await new ComposerWorker(dialogAssist, input, mergingService, logbook).Compose(
                directory,
                compositionProfile,
                deleteConverted,
                searchSubDirectories
            );
        }
        #endregion
    }

    /// <summary>
    /// Worker class for handling compositions.
    /// </summary>
    internal class ComposerWorker : LoggingEnabled<ComposerWorker>
    {
        #region DI Services
        private IDialogAssist dialogAssist;
        private IPathSelection input;
        private IMergingService mergingService;
        #endregion

        #region Fields and properties
        private ProgressDialog progressDialog;
        private IProgress<int>? totalProgress;
        private IProgress<int>? partProgress;

        private CancellationTokenSource cancelSource;
        private CancellationToken cancelToken;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a worker to handle composition for <see cref="Composer"/>.
        /// </summary>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="input">Service for getting path input from user.</param>
        /// <param name="mergingService">Service for merging files into one document.</param>
        /// <param name="logbook">Logging services.</param>
        public ComposerWorker(
            IDialogAssist dialogAssist,
            IPathSelection input,
            IMergingService mergingService,
            ILogbook logbook
        ) : base(logbook)
        {
            // Assign DI services
            this.dialogAssist = dialogAssist;
            this.input = input;
            this.mergingService = mergingService;

            // Create tokens for process cancellation

            cancelSource = new CancellationTokenSource();
            cancelToken = cancelSource.Token;

            // Create an initial progress dialog
            progressDialog = new ProgressDialog(string.Empty, cancelSource)
            {
                TotalPercent = 0,
                PartPercent = 0,
                Phase = Resources.Operations.PhaseNames.EvaluatingFiles
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Compose files contained in a directory (and possibly subdirectories) into a new
        /// document according to rules laid out in given profile.
        /// </summary>
        /// <param name="directory">Directory to search files in.</param>
        /// <param name="compositionProfile">Profile to compose the document by.</param>
        /// <param name="deleteConverted">If true, delete converted files when done.</param>
        /// <param name="searchSubDirectories">If true, also search subdirectories within <paramref name="directory"/>.</param>
        /// <returns>An awaitable task.</returns>
        public async Task Compose(
            string directory,
            ICompositionProfile compositionProfile,
            bool deleteConverted,
            bool searchSubDirectories
        )
        {
            // Cancellation token and source for cancelling and monitoring cancellation status.

            cancelSource = new CancellationTokenSource();
            cancelToken = cancelSource.Token;

            // Create a dialog with information on current progress status.

            progressDialog = new ProgressDialog(string.Empty, cancelSource)
            {
                TotalPercent = 0,
                PartPercent = 0,
                Phase = Resources.Operations.PhaseNames.EvaluatingFiles
            };

            // Show progress to the user

            Task progress = dialogAssist.Show(progressDialog);

            // Start composing

            Task compose = ComposeInternal(
                directory,
                compositionProfile,
                deleteConverted,
                searchSubDirectories
            );

            // Proceed when composing has finished and user has acknowledged this.
            // If an exception is thrown, show an error message instead.

            try
            {
                await Task.WhenAll(progress, compose);
            }
            catch (ArgumentNullException e) when (e.Message == nameof(directory))
            {
                MessageDialog dialog = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.Composition.PathNull
                );
                await dialogAssist.Show(dialog);

                logbook.Write(
                    $"Internal composition method threw an exception.",
                    LogLevel.Error,
                    e
                );

                return;
            }
            catch (ArgumentNullException e) when (e.Message == nameof(compositionProfile))
            {
                MessageDialog dialog = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.Composition.ProfileNotExists
                );
                await dialogAssist.Show(dialog);

                logbook.Write(
                    $"Internal composition method threw an exception.",
                    LogLevel.Error,
                    e
                );

                return;
            }
            catch (DirectoryNotFoundException e)
            {
                MessageDialog dialog = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.Composition.PathNotExists
                );
                await dialogAssist.Show(dialog);

                logbook.Write(
                    $"Internal composition method threw an exception.",
                    LogLevel.Error,
                    e
                );

                return;
            }
        }

        /// <summary>
        /// Internal composing method for handling composition.
        /// </summary>
        /// <param name="directory">Directory to look the files in.</param>
        /// <param name="compositionProfile">Profile to compose the document by.</param>
        /// <param name="deleteConverted">If true, delete converted files when done.</param>
        /// <param name="searchSubDirectories">If true, search for subdirectories inside <paramref name="directory"/>.</param>
        /// <returns>An awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Thrown, if the given directory or profile is null.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown, if no directory with given path is found.</exception>
        protected async Task ComposeInternal(
            string directory,
            ICompositionProfile compositionProfile,
            bool deleteConverted,
            bool searchSubDirectories
        )
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(nameof(directory));
            if (compositionProfile == null)
                throw new ArgumentNullException(nameof(compositionProfile));

            // Determine search criteria and do the search.

            SearchOption subDirs =
                searchSubDirectories == true
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

            IEnumerable<string> files = Directory
                .GetFiles(directory, "*.*", subDirs)
                .Where(
                    x =>
                        x.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                        || x.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)
                        || x.EndsWith(".doc", StringComparison.OrdinalIgnoreCase)
                );

            // Update progress info.

            int totalAmount = 100;
            foreach (ICompositionSegment cs in compositionProfile.Segments)
            {
                if (cs is ICompositionFile)
                {
                    totalAmount += files.Count();
                }
                else if (cs is ICompositionTitle)
                {
                    totalAmount++;
                }
            }
            int currentTotal = 0;

            totalProgress = new Progress<int>(addition =>
            {
                currentTotal += addition;
                progressDialog.TotalPercent = currentTotal * 100 / totalAmount;
            });

            // Add inputs in the collection for merging.

            List<IMergeInput> inputs = new List<IMergeInput>();
            foreach (ICompositionSegment segment in compositionProfile.Segments)
            {
                List<IMergeInput> segmentInput = await EvaluateSegment(segment, files);
                if (cancelToken.IsCancellationRequested)
                    return;
                inputs.AddRange(segmentInput);
            }

            // Remove titles that have no children.

            RemoveEmptyTitles(inputs);

            if (cancelToken.IsCancellationRequested)
            {
                logbook.Write(
                    $"Cancellation requested at token '{cancelToken.GetHashCode()}'.",
                    LogLevel.Debug
                );
                return;
            }

            // Compose the document.

            await ExecuteComposition(
                directory,
                inputs,
                compositionProfile.AddPageNumbers,
                deleteConverted,
                cancelToken
            );

            if (cancelToken.IsCancellationRequested)
            {
                logbook.Write(
                    $"Cancellation requested at token '{cancelToken.GetHashCode()}'.",
                    LogLevel.Debug
                );
                return;
            }

            progressDialog.Phase = ProgressPhase.Finished.GetResourceString();
            progressDialog.Part = null;
        }

        /// <summary>
        /// Determine the type of the segment and evaluate accordingly (see any files match the search criteria).
        /// </summary>
        /// <param name="segment">Segment against which to evaluate the files.</param>
        /// <param name="files">Files to evaluate against the segment rules.</param>
        /// <returns></returns>
        private async Task<List<IMergeInput>> EvaluateSegment(
            ICompositionSegment segment,
            IEnumerable<string> files
        )
        {
            if (segment is ICompositionFile fileSegment)
            {
                return await EvaluateFileSegment(fileSegment, files);
            }
            else if (segment is ICompositionTitle titleSegment)
            {
                totalProgress?.Report(1);
                return new List<IMergeInput>()
                {
                    new MergeInput(null, titleSegment.SegmentName, titleSegment.Level)
                };
            }
            else
            {
                return new List<IMergeInput>();
            }
        }

        /// <summary>
        /// Evaluate files against a file segment and see if any of them fulfill the criteria.
        /// </summary>
        /// <param name="fileSegment">Segment to compare against.</param>
        /// <param name="files">Files to use in the comparison.</param>
        /// <returns></returns>
        private async Task<List<IMergeInput>> EvaluateFileSegment(
            ICompositionFile fileSegment,
            IEnumerable<string> files
        )
        {
            // Display progress.

            int currentPartTotal = 0;
            progressDialog.PartPercent = 0;
            progressDialog.Part = fileSegment.SegmentName;

            partProgress = new Progress<int>(addition =>
            {
                currentPartTotal += addition;
                progressDialog.PartPercent = currentPartTotal * 100 / files.Count();
            });

            // Get results for each file and store results in a list.

            List<IFileEvaluationResult> evaluationResults = new List<IFileEvaluationResult>();
            foreach (string file in files)
            {
                IFileEvaluationResult result = fileSegment.EvaluateFile(file);
                if (result.Outcome == OutcomeType.Match)
                {
                    evaluationResults.Add(result);
                }

                partProgress.Report(1);
                totalProgress?.Report(1);
            }

            // Check for the correct amounts of matching files and
            // ask to choose correct files if appropriate.

            if (fileSegment.MaxCount == 0)
            {
                if (evaluationResults.Count >= fileSegment.MinCount)
                {
                    return AddFileMerges(evaluationResults, fileSegment);
                }
                else
                {
                    return await AskForCorrectFiles(evaluationResults, fileSegment);
                }
            }
            else
            {
                if (
                    evaluationResults.Count >= fileSegment.MinCount
                    && evaluationResults.Count <= fileSegment.MaxCount
                )
                {
                    return AddFileMerges(evaluationResults, fileSegment);
                }
                else
                {
                    return await AskForCorrectFiles(evaluationResults, fileSegment);
                }
            }
        }

        /// <summary>
        /// Add file to merge inputs with the correct name.
        /// </summary>
        /// <param name="results">List containing all evaluation results.</param>
        /// <param name="fileSegment">Segment to which this merge applies.</param>
        /// <returns>List of all inputs with regards to segment.</returns>
        private List<IMergeInput> AddFileMerges(
            IList<IFileEvaluationResult> results,
            ICompositionFile fileSegment
        )
        {
            progressDialog.PartPercent = 0;
            List<IMergeInput> mergeInputs = new List<IMergeInput>();
            foreach (IFileEvaluationResult result in results)
            {
                string name =
                    fileSegment.NameFromFile == true ? result.Name : fileSegment.SegmentName;
                mergeInputs.Add(new MergeInput(result.FilePath, name, fileSegment.Level));
            }

            return mergeInputs;
        }

        /// <summary>
        /// If an incorrect amount of matching files are found, ask the user to provide more files
        /// or remove some files from the list.
        /// </summary>
        /// <param name="results">Evaluation results for this segment.</param>
        /// <param name="fileSegment">Segment the results were evaluated against.</param>
        /// <returns>An awaitable task. The task return a list of merge inputs after choices have been made.</returns>
        private async Task<List<IMergeInput>> AskForCorrectFiles(
            IList<IFileEvaluationResult> results,
            ICompositionFile fileSegment
        )
        {
            progressDialog.PartPercent = 0;
            CompositionFileCountDialog countDialog = new CompositionFileCountDialog(
                results,
                fileSegment,
                input,
                Resources.Labels.Dialogs.CompositionFileCount.SearchResult
            );
            await dialogAssist.Show(countDialog);
            if (countDialog.IsCanceled)
            {
                progressDialog.Close.Execute(null);
                return new List<IMergeInput>();
            }
            return AddFileMerges(countDialog.Results, fileSegment);
        }

        /// <summary>
        /// Remove titles without children from the merge inputs list.
        /// </summary>
        /// <param name="inputs">Inputs to check.</param>
        private void RemoveEmptyTitles(List<IMergeInput> inputs)
        {
            progressDialog.PartPercent = 0;
            int currentAmount = 0;

            for (int i = inputs.Count - 1; i >= 0; i--)
            {
                IMergeInput current = inputs[i];

                if (current.FilePath != null)
                {
                    currentAmount++;
                    progressDialog.PartPercent = currentAmount / inputs.Count * 100;
                    continue;
                }

                if (i == inputs.Count - 1)
                {
                    inputs.RemoveAt(i);
                    currentAmount++;
                    progressDialog.PartPercent = currentAmount / inputs.Count * 100;
                    continue;
                }

                if (inputs[i + 1].Level <= current.Level)
                {
                    inputs.RemoveAt(i);
                    currentAmount++;
                    progressDialog.PartPercent = currentAmount / inputs.Count * 100;
                    continue;
                }
            }
        }

        /// <summary>
        /// Execute the actual composition.
        /// </summary>
        /// <param name="directory">Directory from which files were looked from.</param>
        /// <param name="inputs">Merge inputs for segments.</param>
        /// <param name="addPageNumbers">If true, add page numbers to product pages.</param>
        /// <param name="deleteConverted">If true, delete converted files when done.</param>
        /// <param name="token">Token for process cancellation info.</param>
        /// <returns>An awaitable task.</returns>
        private async Task ExecuteComposition(
            string directory,
            List<IMergeInput> inputs,
            bool addPageNumbers,
            bool deleteConverted,
            CancellationToken token
        )
        {
            progressDialog.PartPercent = 0;
            progressDialog.Part = Resources.Operations.PhaseNames.ChoosingDestination;
            progressDialog.Phase = Resources.Operations.PhaseNames.Merging;

            int alreadyReportedPercent = 0;

            IProgress<ProgressReport> mergeProgress = new Progress<ProgressReport>(progress =>
            {
                progressDialog.PartPercent = progress.Percentage;
                progressDialog.Part = progress.CurrentPhase.GetResourceString();
                totalProgress?.Report(progress.Percentage - alreadyReportedPercent);
                alreadyReportedPercent = progress.Percentage;
            });

            string filePath = await Task.Run(
                () =>
                    input.SaveFile(
                        Resources.UserInput.Descriptions.SelectSaveFile,
                        FileType.PDF,
                        new DirectoryInfo(directory).Name + ".pdf"
                    )
            );

            if (string.IsNullOrEmpty(filePath))
            {
                progressDialog.Close.Execute(null);
                return;
            }

            FileInfo output = new FileInfo(filePath);

            bool convertWord = inputs
                .Where(f => string.IsNullOrEmpty(f.FilePath) == false)
                .Any(i => Path.GetExtension(i.FilePath).ToLower().Contains(".doc"));

            MergingOptions options = new MergingOptions(
                inputs,
                output,
                addPageNumbers,
                convertWord
            );
            options.Cancellation = token;
            options.Progress = mergeProgress;

            IList<FileSystemInfo>? created = null;
            try
            {
                created = await mergingService.MergeWithOptions(options);
            }
            catch (Exception e)
            {
                logbook.Write($"Merging failed.", LogLevel.Error, e);
                throw;
            }

            if (created == null)
            {
                return;
            }

            if (cancelToken.IsCancellationRequested == false)
            {
                FileSystemInfo? info = created.FirstOrDefault(x => x.FullName == filePath);
                if (info != null)
                    created.Remove(info);
            }

            if (deleteConverted || cancelToken.IsCancellationRequested)
            {
                foreach (FileSystemInfo file in created)
                {
                    if (file.Exists)
                        file.Delete();
                }
            }
        }
        #endregion
    }
}
