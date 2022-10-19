using Opus.Services.Data.Composition;
using Opus.Services.Implementation.Base;
using Prism.Commands;
using System.Diagnostics;
using System.IO;

namespace Opus.Services.Implementation.Data.Composition
{
    /// <summary>
    /// Wrapper for evaluation result and file-related extra info.
    /// </summary>
    public class EvaluationWrapper : SelectableBase
    {
        /// <summary>
        /// The result of an evaluation against a segment.
        /// </summary>
        public IFileEvaluationResult Result { get; }

        /// <summary>
        /// Name of the file without extension.
        /// </summary>
        public string Name => Path.GetFileNameWithoutExtension(Result.FilePath);

        /// <summary>
        /// Name of the file with extension.
        /// </summary>
        public string FileName => Path.GetFileName(Result.FilePath);

        /// <summary>
        /// Path of the file.
        /// </summary>
        public string FilePath => Result.FilePath;

        /// <summary>
        /// Command for opening the file.
        /// </summary>
        public DelegateCommand OpenFile { get; }

        /// <summary>
        ///  Create a new wrapper.
        /// </summary>
        /// <param name="result">Result of the evaluation.</param>
        public EvaluationWrapper(IFileEvaluationResult result)
        {
            Result = result;
            OpenFile = new DelegateCommand(ExecuteOpenFile);
        }

        /// <summary>
        /// Create a new wrapper.
        /// </summary>
        /// <param name="filePath">Path of the file to evaluate.</param>
        public EvaluationWrapper(string filePath)
        {
            Result = EvaluationResult.Match(filePath, Path.GetFileNameWithoutExtension(filePath));
            OpenFile = new DelegateCommand(ExecuteOpenFile);
        }

        /// <summary>
        /// Execution method for file opening command, see <see cref="OpenFile"/>.
        /// </summary>
        private void ExecuteOpenFile()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(Result.FilePath) { UseShellExecute = true };
            p.Start();
        }
    }
}
