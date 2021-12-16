using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CX.PdfLib.Common;
using CX.PdfLib.Services.Data;
using Opus.Services.Data;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Input;
using Opus.Services.UI;

namespace Opus.Services.Implementation.Data
{
    public class Composer : ICompositor
    {
        private IDialogAssist dialogAssist;
        private IPathSelection input;
        private ProgressDialog progressDialog;
        private IProgress<int>? totalProgress;
        private IProgress<int>? partProgress;

        public Composer(IDialogAssist dialog, IPathSelection input)
        {
            this.dialogAssist = dialog;
            this.input = input;
            progressDialog = new ProgressDialog()
            {
                TotalPercent = 0,
                PartPercent = 0,
                Phase = Resources.CompositionPhases.EvaluatingFiles,
                Part = Resources.CompositionParts.Wait
            };
        }

        public async Task Compose(string directory, ICompositionProfile compositionProfile)
        {
            progressDialog = new ProgressDialog()
            {
                TotalPercent = 0,
                PartPercent = 0,
                Phase = Resources.CompositionPhases.EvaluatingFiles,
                Part = Resources.CompositionParts.Wait
            };

            Task progress = dialogAssist.Show(progressDialog);
            Task compose = ComposeInternal(directory, compositionProfile);

            await progress;
        }

        private async Task ComposeInternal(string directory, ICompositionProfile compositionProfile)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));
            if (!Directory.Exists(directory))
                throw new ArgumentException(nameof(directory));
            if (compositionProfile == null)
                throw new ArgumentNullException(nameof(compositionProfile));

            IEnumerable<string> files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                .Where(x => x.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                x.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
                x.EndsWith(".doc", StringComparison.OrdinalIgnoreCase));

            int totalAmount = 100;
            foreach (ICompositionSegment cs in compositionProfile.Segments.Items)
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
                progressDialog.TotalPercent = currentTotal / totalAmount * 100;
            });

            List<IMergeInput> inputs = new List<IMergeInput>();
            foreach (ICompositionSegment segment in compositionProfile.Segments.Items)
            {
                List<IMergeInput> segmentInput = await EvaluateSegment(segment, files);
                if (progressDialog.IsCanceled)
                    return;
                inputs.AddRange(segmentInput);
            }

            RemoveEmptyTitles(inputs);
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
                progressDialog.PartPercent = currentPartTotal / files.Count() * 100;
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
            CompositionFileCountDialog countDialog = new CompositionFileCountDialog(results, fileSegment, input, dialogAssist);
            await dialogAssist.Show(countDialog);
            if (countDialog.IsCanceled)
            {
                progressDialog.Close.Execute(null);
                return new List<IMergeInput>();
            }
            return AddFileMerges(countDialog.Files.ToList(), fileSegment);
        }

        private void RemoveEmptyTitles(List<IMergeInput> inputs)
        {
            progressDialog.PartPercent = 0;
            for (int i = inputs.Count - 1; i >= 0; i--)
            {
                IMergeInput current = inputs[i];

                if (current.FilePath != null)
                {
                    continue;
                }

                if (i == inputs.Count - 1)
                {
                    inputs.RemoveAt(i);
                    continue;
                }

                if (inputs[i + 1].Level <= current.Level)
                {
                    inputs.RemoveAt(i);
                    continue;
                }
            }
        }

        private void ExecuteComposition()
        {

        }
    }
}
