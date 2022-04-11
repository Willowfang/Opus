using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using Opus.Services.Data;
using Opus.Services.Data.Composition;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Input;
using Opus.Services.UI;
using Opus.Services.Extensions;
using CX.LoggingLib;

namespace Opus.Services.Implementation.Data.Composition
{
    public class Composer : IComposer
    {
        private IDialogAssist dialogAssist;
        private IPathSelection input;
        private ILogbook logbook;
        private IMergingService mergingService;

        public Composer(
            IDialogAssist dialogAssist, 
            IPathSelection input,
            IMergingService mergingService,
            ILogbook logbook)
        {
            this.dialogAssist = dialogAssist;
            this.input = input;
            this.logbook = logbook;
            this.mergingService = mergingService;
        }

        public async Task Compose(string directory, ICompositionProfile compositionProfile,
            bool deleteConverted, bool searchSubDirectories)
        {
            await new ComposerWorker(dialogAssist, input, mergingService, logbook)
                .Compose(directory, compositionProfile, deleteConverted, searchSubDirectories);
        }
    }

    internal class ComposerWorker : LoggingEnabled<ComposerWorker>
    {
        /// <summary>
        /// Service for showing dialogs
        /// </summary>
        private IDialogAssist dialogAssist;
        /// <summary>
        /// Service for getting user input
        /// </summary>
        private IPathSelection input;
        private IMergingService mergingService;
        /// <summary>
        /// Dialog model for showing progress
        /// </summary>
        private ProgressDialog progressDialog;
        /// <summary>
        /// Progress reporting for total progress of composition
        /// </summary>
        private IProgress<int>? totalProgress;
        /// <summary>
        /// Progress reporting for the current part being processed
        /// </summary>
        private IProgress<int>? partProgress;

        private CancellationTokenSource cancelSource;
        private CancellationToken cancelToken;

        /// <summary>
        /// Create an implementation instance for <see cref="IComposerDefunct"/>
        /// </summary>
        /// <param name="dialogAssist">Service for showing <see cref="IDialog"/> instances</param>
        /// <param name="input">Service for retrieving user input</param>
        /// <param name="manipulator">Service for manipulating pdf files</param>
        public ComposerWorker(
            IDialogAssist dialogAssist, 
            IPathSelection input,
            IMergingService mergingService,
            ILogbook logbook) : base(logbook)
        {
            this.dialogAssist = dialogAssist;
            this.input = input;
            this.mergingService = mergingService;
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

        /// <summary>
        /// Interface implementation for composing a pdf file from separate files
        /// </summary>
        /// <param name="directory">Directory (and subdirectories) to search files from</param>
        /// <param name="compositionProfile">Profile containing the <see cref="ICompositionSegment"/>s to do
        /// the composition by</param>
        /// <returns></returns>
        public async Task Compose(string directory, ICompositionProfile compositionProfile,
            bool deleteConverted, bool searchSubDirectories)
        {
            cancelSource = new CancellationTokenSource();
            cancelToken = cancelSource.Token;

            progressDialog = new ProgressDialog(string.Empty, cancelSource)
            {
                TotalPercent = 0,
                PartPercent = 0,
                Phase = Resources.Operations.PhaseNames.EvaluatingFiles
            };

            // Show progress to the user
            Task progress = dialogAssist.Show(progressDialog);

            // Start composing
            Task compose = ComposeInternal(directory, compositionProfile, deleteConverted, searchSubDirectories);

            try
            {
                await Task.WhenAll(progress, compose);
            }
            catch (ArgumentNullException e) when (e.Message == nameof(directory))
            {
                MessageDialog dialog = new MessageDialog(Resources.Labels.General.Error, Resources.Messages.Composition.PathNull);
                await dialogAssist.Show(dialog);

                logbook.Write($"Internal composition method threw an exception.", LogLevel.Error, e);

                return;
            }
            catch (ArgumentNullException e) when (e.Message == nameof(compositionProfile))
            {
                MessageDialog dialog = new MessageDialog(Resources.Labels.General.Error,
                    Resources.Messages.Composition.ProfileNotExists);
                await dialogAssist.Show(dialog);

                logbook.Write($"Internal composition method threw an exception.", LogLevel.Error, e);

                return;
            }
            catch (DirectoryNotFoundException e)
            {
                MessageDialog dialog = new MessageDialog(Resources.Labels.General.Error,
                    Resources.Messages.Composition.PathNotExists);
                await dialogAssist.Show(dialog);

                logbook.Write($"Internal composition method threw an exception.", LogLevel.Error, e);

                return;
            }
        }

        private async Task ComposeInternal(string directory, ICompositionProfile compositionProfile,
            bool deleteConverted, bool searchSubDirectories)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(nameof(directory));
            if (compositionProfile == null)
                throw new ArgumentNullException(nameof(compositionProfile));

            SearchOption subDirs = searchSubDirectories == true ? 
                SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            IEnumerable<string> files = Directory.GetFiles(directory, "*.*", subDirs)
                .Where(x => x.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                x.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
                x.EndsWith(".doc", StringComparison.OrdinalIgnoreCase));

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

            List<IMergeInput> inputs = new List<IMergeInput>();
            foreach (ICompositionSegment segment in compositionProfile.Segments)
            {
                List<IMergeInput> segmentInput = await EvaluateSegment(segment, files);
                if (cancelToken.IsCancellationRequested)
                    return;
                inputs.AddRange(segmentInput);
            }

            RemoveEmptyTitles(inputs);

            if (cancelToken.IsCancellationRequested)
            {
                logbook.Write($"Cancellation requested at token '{cancelToken.GetHashCode()}'.", LogLevel.Debug);
                return;
            }

            await ExecuteComposition(directory, inputs, compositionProfile.AddPageNumbers,
                deleteConverted, cancelToken);

            if (cancelToken.IsCancellationRequested)
            {
                logbook.Write($"Cancellation requested at token '{cancelToken.GetHashCode()}'.", LogLevel.Debug);
                return;
            }

            progressDialog.Phase = ProgressPhase.Finished.GetResourceString();
            progressDialog.Part = null;
        }

        private async Task<List<IMergeInput>> EvaluateSegment(ICompositionSegment segment, IEnumerable<string> files)
        {
            if (segment is ICompositionFile fileSegment)
            {
                return await EvaluateFileSegment(fileSegment, files);
            }
            else if (segment is ICompositionTitle titleSegment)
            {
                totalProgress?.Report(1);
                return new List<IMergeInput>() { new MergeInput(null, titleSegment.SegmentName, titleSegment.Level) };
            }
            else
            {
                return new List<IMergeInput>();
            }
        }

        private async Task<List<IMergeInput>> EvaluateFileSegment(ICompositionFile fileSegment, IEnumerable<string> files)
        {
            int currentPartTotal = 0;
            progressDialog.PartPercent = 0;
            progressDialog.Part = fileSegment.SegmentName;

            partProgress = new Progress<int>(addition =>
            {
                currentPartTotal += addition;
                progressDialog.PartPercent = currentPartTotal * 100 / files.Count();
            });

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
                if (evaluationResults.Count >= fileSegment.MinCount && evaluationResults.Count <= fileSegment.MaxCount)
                {
                    return AddFileMerges(evaluationResults, fileSegment);
                }
                else
                {
                    return await AskForCorrectFiles(evaluationResults, fileSegment);
                }
            }
        }

        private List<IMergeInput> AddFileMerges(IList<IFileEvaluationResult> results, ICompositionFile fileSegment)
        {
            progressDialog.PartPercent = 0;
            List<IMergeInput> mergeInputs = new List<IMergeInput>();
            foreach (IFileEvaluationResult result in results)
            {
                string name = fileSegment.NameFromFile == true ? result.Name : fileSegment.SegmentName;
                mergeInputs.Add(new MergeInput(result.FilePath, name, fileSegment.Level));
            }

            return mergeInputs;
        }

        private async Task<List<IMergeInput>> AskForCorrectFiles(IList<IFileEvaluationResult> results, ICompositionFile fileSegment)
        {
            progressDialog.PartPercent = 0;
            CompositionFileCountDialog countDialog = new CompositionFileCountDialog(results, fileSegment, input, Resources.Labels.Dialogs.CompositionFileCount.SearchResult);
            await dialogAssist.Show(countDialog);
            if (countDialog.IsCanceled)
            {
                progressDialog.Close.Execute(null);
                return new List<IMergeInput>();
            }
            return AddFileMerges(countDialog.Results, fileSegment);
        }

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

        private async Task ExecuteComposition(string directory, List<IMergeInput> inputs, bool addPageNumbers, 
            bool deleteConverted, CancellationToken token)
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

            string filePath = await Task.Run(() => input.SaveFile(Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.PDF, new DirectoryInfo(directory).Name + ".pdf"));

            if (string.IsNullOrEmpty(filePath))
            {
                progressDialog.Close.Execute(null);
                return;
            }

            FileInfo output = new FileInfo(filePath);

            bool convertWord = inputs.Where(f => string.IsNullOrEmpty(f.FilePath) == false).Any(i => Path.GetExtension(i.FilePath).ToLower().Contains(".doc"));

            MergingOptions options = new MergingOptions(inputs, output, addPageNumbers, convertWord);
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
    }
}
