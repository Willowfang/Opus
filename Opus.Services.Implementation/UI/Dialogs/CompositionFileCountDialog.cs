using Opus.Services.Data;
using Opus.Services.Implementation.Data;
using Opus.Services.Implementation.UI;
using Opus.Services.Input;
using Opus.Services.UI;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class CompositionFileCountDialog : DialogBase, IDialog
    {
        public ObservableCollection<IFileEvaluationResult> Files { get; set; }
        public ICompositionFile Segment { get; set; }

        private IPathSelection input;
        private IDialogAssist dialog;

        private IFileEvaluationResult? selectedFile;
        public IFileEvaluationResult? SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        public DelegateCommand AddFiles { get; }
        public DelegateCommand RemoveFile { get; }

        public CompositionFileCountDialog(IList<IFileEvaluationResult> files, ICompositionFile fileSegment, 
            IPathSelection input, IDialogAssist dialog)
        {
            AddFiles = new DelegateCommand(ExecuteAddFiles);
            RemoveFile = new DelegateCommand(ExecuteRemoveFile);
            Files = new ObservableCollection<IFileEvaluationResult>(files);
            Segment = fileSegment;
            this.input = input;
            this.dialog = dialog;
            Files.CollectionChanged += FilePaths_CollectionChanged;
        }

        private void FilePaths_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void ExecuteAddFiles()
        {
            string[] paths = input.OpenFiles(Resources.Labels.CompositionCountDialog_AddFiles,
                new List<FileType>() { FileType.PDF, FileType.Word });
            int pathCount = paths.Count();
            if (pathCount + Files.Count > Segment.MaxCount)
            {
                dialog.Show(new MessageDialog(string.Format(
                    Resources.Labels.CompositionCountDialog_TooManySelected, Segment.SegmentName, Segment.MaxCount)));
                return;
            }

            foreach (string path in paths)
            {
                Files.Add(EvaluationResult.Match(path, Path.GetFileNameWithoutExtension(path)));
            }
        }

        private void ExecuteRemoveFile()
        {
            if (SelectedFile == null)
            {
                return;
            }    

            Files.Remove(SelectedFile);
        }

        protected override bool SaveCanExecute()
        {
            if (Segment.MaxCount > 0)
            {
                return Files.Count >= Segment.MinCount && Files.Count <= Segment.MaxCount;
            }
            else
            {
                return Files.Count >= Segment.MinCount;
            }
        }
    }
}
