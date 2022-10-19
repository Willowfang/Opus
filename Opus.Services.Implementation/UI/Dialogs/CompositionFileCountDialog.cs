using CX.LoggingLib;
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
    /// <summary>
    /// Dialog for adding or removing found files in the composition process.
    /// </summary>
    public class CompositionFileCountDialog : DialogBase, IDialog
    {
        #region DI services
        private readonly IPathSelection input;
        #endregion

        #region Fields and properties
        /// <summary>
        /// Files found or added.
        /// </summary>
        public ObservableCollection<EvaluationWrapper> Files { get; set; }

        /// <summary>
        /// The composition segment this dialog is about.
        /// </summary>
        public ICompositionFile Segment { get; set; }

        private EvaluationWrapper? selectedFile;

        /// <summary>
        /// Currently selected file.
        /// </summary>
        public EvaluationWrapper? SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        /// <summary>
        /// If true, only a single file can be selected at a time.
        /// </summary>
        public bool SingleSelection { get; }

        /// <summary>
        /// Instructions or other textual content for the dialog.
        /// </summary>
        public string DialogTextContent { get; }

        /// <summary>
        /// The results after dialog has been presented.
        /// </summary>
        public IList<IFileEvaluationResult> Results
        {
            get => Files.Select(x => x.Result).ToList();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command for adding files.
        /// </summary>
        public DelegateCommand AddFiles { get; }

        /// <summary>
        /// Command for adding a single file.
        /// </summary>
        public DelegateCommand AddFile { get; }

        /// <summary>
        /// Command for removing a file.
        /// </summary>
        public DelegateCommand RemoveFiles { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new file count dialog.
        /// </summary>
        /// <param name="files">Files to display in the dialog list.</param>
        /// <param name="fileSegment">Segment this dialog is about.</param>
        /// <param name="input">Service for getting a path as user input.</param>
        /// <param name="dialogTitle">Title of this dialog.</param>
        public CompositionFileCountDialog(
            IList<IFileEvaluationResult> files,
            ICompositionFile fileSegment,
            IPathSelection input,
            string dialogTitle
        ) : base(dialogTitle)
        {
            // Assign commands.

            AddFiles = new DelegateCommand(ExecuteAddFiles);
            RemoveFiles = new DelegateCommand(ExecuteRemoveFiles);
            AddFile = new DelegateCommand(ExecuteAddFile);

            // Initialize list.

            Files = new ObservableCollection<EvaluationWrapper>();

            foreach (var file in files)
            {
                Files.Add(new EvaluationWrapper(file));
            }

            Segment = fileSegment;
            this.input = input;

            Files.CollectionChanged += FilePaths_CollectionChanged;

            DialogTextContent = Resources.Validation.Composition.WrongNumberOfFiles;

            if (files.Count == 0 && fileSegment.MinCount == 1 && fileSegment.MaxCount == 1)
            {
                SingleSelection = true;
                DialogTextContent = Resources.Validation.Composition.OneFileNotFound;
            }
        }
        #endregion

        #region EventHandlers
        /// <summary>
        /// Handle files collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilePaths_CollectionChanged(
            object? sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
        {
            SaveCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Execution method for add files command, see <see cref="AddFiles"/>.
        /// </summary>
        private void ExecuteAddFiles()
        {
            string[] paths = input.OpenFiles(
                Resources.UserInput.Descriptions.SelectOpenFiles,
                new List<FileType>() { FileType.PDF, FileType.Word }
            );

            foreach (string path in paths)
            {
                Files.Add(new EvaluationWrapper(path));
            }
        }

        /// <summary>
        /// Execution method for add file command, see <see cref="AddFile"/>.
        /// </summary>
        private void ExecuteAddFile()
        {
            string path = input.OpenFile(
                Resources.UserInput.Descriptions.SelectOpenFile,
                FileType.PDF
            );

            if (Files.Count == 0)
            {
                Files.Add(new EvaluationWrapper(path));
            }
            else
            {
                Files[0] = new EvaluationWrapper(path);
            }
        }

        /// <summary>
        /// Execution method for remove files command, see <see cref="ExecuteRemoveFiles"/>.
        /// </summary>
        private void ExecuteRemoveFiles()
        {
            Files.RemoveAll(x => x.IsSelected);
        }
        #endregion

        #region Overrides
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
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
        #endregion
    }
}
