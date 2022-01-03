using Opus.Services.Data.Composition;
using Opus.Services.Extensions;
using Opus.Services.Implementation.Data.Composition;
using Opus.Services.Input;
using Opus.Services.UI;
using Prism.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class CompositionFileCountDialog : DialogBase, IDialog
    {
        public ObservableCollection<EvaluationWrapper> Files { get; set; }
        public ICompositionFile Segment { get; set; }

        private IPathSelection input;

        private EvaluationWrapper? selectedFile;
        public EvaluationWrapper? SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        public DelegateCommand AddFiles { get; }
        public DelegateCommand AddFile { get; }
        public DelegateCommand RemoveFiles { get; }

        public bool SingleSelection { get; }
        public string DialogTextContent { get; }

        public IList<IFileEvaluationResult> Results
        {
            get => Files.Select(x => x.Result).ToList();
        }

        public CompositionFileCountDialog(IList<IFileEvaluationResult> files, ICompositionFile fileSegment, 
            IPathSelection input, string dialogTitle)
            : base(dialogTitle)
        {
            AddFiles = new DelegateCommand(ExecuteAddFiles);
            RemoveFiles = new DelegateCommand(ExecuteRemoveFiles);
            AddFile = new DelegateCommand(ExecuteAddFile);
            Files = new ObservableCollection<EvaluationWrapper>();
            foreach (var file in files)
            {
                Files.Add(new EvaluationWrapper(file));
            }
            Segment = fileSegment;
            this.input = input;
            Files.CollectionChanged += FilePaths_CollectionChanged;

            DialogTextContent = Resources.Validation.Composition.WrongNumberOfFiles;

            if (files.Count == 0 && fileSegment.MinCount == 1 &&
                fileSegment.MaxCount == 1)
            {
                SingleSelection = true;
                DialogTextContent = Resources.Validation.Composition.OneFileNotFound;
            }
        }

        private void FilePaths_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void ExecuteAddFiles()
        {
            string[] paths = input.OpenFiles(Resources.UserInput.Descriptions.SelectOpenFiles,
                new List<FileType>() { FileType.PDF, FileType.Word });

            foreach (string path in paths)
            {
                Files.Add(new EvaluationWrapper(path));
            }
        }

        private void ExecuteAddFile()
        {
            string path = input.OpenFile(Resources.UserInput.Descriptions.SelectOpenFile,
                FileType.PDF);

            if (Files.Count == 0)
            {
                Files.Add(new EvaluationWrapper(path));
            }
            else
            {
                Files[0] = new EvaluationWrapper(path);
            }
        }

        private void ExecuteRemoveFiles()
        {
            Files.RemoveAll(x => x.IsSelected);
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
