using Opus.Services.Data.Composition;
using Opus.Services.Implementation.Base;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Data.Composition
{
    public class EvaluationWrapper : SelectableBase
    {
        public IFileEvaluationResult Result { get; }

        public string Name => Path.GetFileNameWithoutExtension(Result.FilePath);
        public string FileName => Path.GetFileName(Result.FilePath);
        public string FilePath => Result.FilePath;

        public DelegateCommand OpenFile { get; }

        public EvaluationWrapper(IFileEvaluationResult result)
        {
            Result = result;
            OpenFile = new DelegateCommand(ExecuteOpenFile);
        }
        public EvaluationWrapper(string filePath)
        {
            Result = EvaluationResult.Match(filePath, Path.GetFileNameWithoutExtension(filePath));
            OpenFile = new DelegateCommand(ExecuteOpenFile);
        }

        private void ExecuteOpenFile()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(Result.FilePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }
    }
}
