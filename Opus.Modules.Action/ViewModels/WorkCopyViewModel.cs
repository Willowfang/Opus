using Opus.Core.Base;
using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Linq;
using CX.PdfLib.Services;
using Opus.Core.Wrappers;
using AsyncAwaitBestPractices.MVVM;
using System.Threading.Tasks;
using Opus.Values;
using Opus.Services.Input;
using Opus.Services.UI;
using Opus.Events;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Extensions;
using System.Threading;
using CX.LoggingLib;
using Opus.Services.Configuration;
using Opus.Core.Executors;
using System.IO;
using System.Collections.Generic;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// ViewModel for creating work copies of pdf-files.
    /// </summary>
    public class WorkCopyViewModel : ViewModelBaseLogging<WorkCopyViewModel>, INavigationTarget
    {
        #region DI services
        private readonly IConfiguration configuration;
        private readonly IPathSelection input;
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogAssist dialogAssist;
        private readonly ISignatureExecutor executor;
        private readonly IAnnotationService annotationService;
        #endregion

        #region Fields and properties
        /// <summary>
        /// Collection for files that will have their signatures removed.
        /// </summary>
        public ObservableCollection<FileStorage> OriginalFiles { get; set; }

        private FileStorage selectedFile;

        /// <summary>
        /// The file that is currently selected.
        /// </summary>
        public FileStorage SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new viewModel for handling work copy creation.
        /// </summary>
        /// <param name="eventAggregator">Service for publishing and receiving events between viewModels.</param>
        /// <param name="executor">Service for executing work copy creation.</param>
        /// <param name="input">Service for getting path information as user input.</param>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="navRegistry">Navigation registry for viewModels.</param>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="annotationService">Service for manipulating pdf annotations.</param>
        /// <param name="logbook">Logging services.</param>
        public WorkCopyViewModel(
            IEventAggregator eventAggregator,
            ISignatureExecutor executor,
            IPathSelection input,
            IConfiguration configuration,
            INavigationTargetRegistry navRegistry,
            IDialogAssist dialogAssist,
            IAnnotationService annotationService,
            ILogbook logbook
        ) : base(logbook)
        {
            // Initialize collection.

            OriginalFiles = new ObservableCollection<FileStorage>();

            // Assign DI services.

            this.configuration = configuration;
            this.input = input;
            this.eventAggregator = eventAggregator;
            this.dialogAssist = dialogAssist;
            this.executor = executor;
            this.annotationService = annotationService;

            // Add this viewmodel as navigation target with proper scheme.

            navRegistry.AddTarget(SchemeNames.WORKCOPY, this);
        }
        #endregion

        #region INavigationTarget implementation

        /// <summary>
        /// Subscription token for filesaddedevent. Stored for unsubscribing when leaving this viewmodel.
        /// </summary>
        SubscriptionToken filesAddedSubscription;

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>. Subscribe to events.
        /// </summary>
        public void OnArrival()
        {
            filesAddedSubscription = eventAggregator
                .GetEvent<FilesAddedEvent>()
                .Subscribe(FilesAdded);

            logbook.Write($"{this} subscribed to {nameof(FilesAddedEvent)}.", LogLevel.Debug);
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>. Unsubscribe from events.
        /// </summary>
        public void WhenLeaving()
        {
            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);

            logbook.Write($"{this} unsubscribed from {nameof(FilesAddedEvent)}.", LogLevel.Debug);
        }

        /// <summary>
        /// When reset is requested by the user, clear files list and deselect any files.
        /// </summary>
        public void Reset()
        {
            OriginalFiles?.Clear();
            SelectedFile = null;
        }

        private void FilesAdded(string[] addedFiles)
        {
            foreach (string file in addedFiles)
            {
                if (!OriginalFiles.Any(f => f.FilePath == file))
                    OriginalFiles.Add(new FileStorage(file));
            }
        }
        #endregion

        #region Commands
        private DelegateCommand _deleteCommand;

        /// <summary>
        /// Command for deleting a file entry from the list.
        /// </summary>
        public DelegateCommand DeleteCommand =>
            _deleteCommand ?? (_deleteCommand = new DelegateCommand(ExecuteDeleteCommand));

        /// <summary>
        /// Execution method for delete command, see <see cref="DeleteCommand"/>.
        /// <para>
        /// Remove an entry from the list.
        /// </para>
        /// </summary>
        protected void ExecuteDeleteCommand()
        {
            OriginalFiles.RemoveAll(x => x.IsSelected);
        }

        private DelegateCommand clearCommand;

        /// <summary>
        /// Command for clearing the whole list.
        /// </summary>
        public DelegateCommand ClearCommand =>
            clearCommand ?? (clearCommand = new DelegateCommand(ExecuteClearCommand));

        /// <summary>
        /// Execution method for clear command, see <see cref="ClearCommand"/>.
        /// <para>
        /// Clear the whole list of file entries.
        /// </para>
        /// </summary>
        protected void ExecuteClearCommand()
        {
            OriginalFiles.Clear();
        }

        private IAsyncCommand createWorkCopyCommand;

        /// <summary>
        /// Command for creating work copies.
        /// </summary>
        public IAsyncCommand CreateWorkCopyCommand =>
            createWorkCopyCommand
            ?? (createWorkCopyCommand = new AsyncCommand(ExecuteCreateWorkCopy));

        /// <summary>
        /// Execution method for work copy creation command, see <see cref="CreateWorkCopyCommand"/>.
        /// <para>
        /// Executes the actual action.
        /// </para>
        /// </summary>
        /// <returns></returns>
        protected async Task ExecuteCreateWorkCopy()
        {
            string path = input.OpenDirectory(Resources.UserInput.Descriptions.SelectSaveFolder);
            if (path == null)
                return;

            logbook.Write($"Started work copy creation.", LogLevel.Information);

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            ProgressDialog dialog = new ProgressDialog(string.Empty, tokenSource)
            {
                TotalPercent = 0,
                Phase = Resources.Operations.PhaseNames.Unassigned
            };

            Task showProgress = dialogAssist.Show(dialog);

            IList<FileInfo> created = await executor.Remove(
                OriginalFiles,
                new DirectoryInfo(path),
                tokenSource
            );

            if (configuration.WorkCopyFlattenRedactions)
            {
                List<Task> redTasks = new List<Task>();
                foreach (FileInfo file in created)
                {
                    redTasks.Add(annotationService.FlattenRedactions(file.FullName));
                }

                await Task.WhenAll(redTasks);
            }

            dialog.TotalPercent = 100;
            dialog.Phase = Resources.Operations.PhaseNames.Finished;

            await showProgress;

            logbook.Write($"Work copy creation finished.", LogLevel.Information);
        }

        #endregion
    }
}
