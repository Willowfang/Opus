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
using CX.PdfLib.Common;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Data;
using Opus.Services.Extensions;
using System.Threading;
using CX.LoggingLib;
using Opus.Services.Configuration;
using Opus.Core.Executors;
using System.IO;
using System.Collections.Generic;

namespace Opus.Modules.Action.ViewModels
{
    public class WorkCopyViewModel : ViewModelBaseLogging<WorkCopyViewModel>, INavigationTarget
    {
        private readonly IConfiguration configuration;
        // Service for getting user input related to file paths
        private readonly IPathSelection input;
        // Prism events service
        private readonly IEventAggregator eventAggregator;
        /// <summary>
        /// Common service for displaying and updating a dialog
        /// </summary>
        private readonly IDialogAssist dialogAssist;
        private readonly ISignatureExecutor executor;
        private readonly IAnnotationService annotationService;

        // Collection for files that will have their signatures removed
        public ObservableCollection<FileStorage> OriginalFiles { get; set; }
        private FileStorage selectedFile;
        // Currently selected file
        public FileStorage SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        public WorkCopyViewModel(
            IEventAggregator eventAggregator, 
            ISignatureExecutor executor,
            IPathSelection input, 
            IConfiguration configuration,
            INavigationTargetRegistry navRegistry, 
            IDialogAssist dialogAssist,
            IAnnotationService annotationService,
            ILogbook logbook) : base(logbook)
        {
            OriginalFiles = new ObservableCollection<FileStorage>();
            this.configuration = configuration;
            this.input = input;
            this.eventAggregator = eventAggregator;
            this.dialogAssist = dialogAssist;
            this.executor = executor;
            this.annotationService = annotationService;
            navRegistry.AddTarget(SchemeNames.WORKCOPY, this);
        }

        SubscriptionToken filesAddedSubscription;
        public void OnArrival()
        {
            filesAddedSubscription = eventAggregator.GetEvent<FilesAddedEvent>().Subscribe(FilesAdded);

            logbook.Write($"{this} subscribed to {nameof(FilesAddedEvent)}.", LogLevel.Debug);
        }
        public void WhenLeaving()
        {
            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);

            logbook.Write($"{this} unsubscribed from {nameof(FilesAddedEvent)}.", LogLevel.Debug);
        }
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

        private DelegateCommand _deleteCommand;
        public DelegateCommand DeleteCommand =>
            _deleteCommand ?? (_deleteCommand = new DelegateCommand(ExecuteDeleteCommand));

        private void ExecuteDeleteCommand()
        {
            OriginalFiles.RemoveAll(x => x.IsSelected);
        }

        private DelegateCommand _clearCommand;
        public DelegateCommand ClearCommand =>
            _clearCommand ?? (_clearCommand = new DelegateCommand(ExecuteClearCommand));

        private void ExecuteClearCommand()
        {
            OriginalFiles.Clear();
        }

        private IAsyncCommand _createWorkCopyCommand;
        public IAsyncCommand CreateWorkCopyCommand =>
            _createWorkCopyCommand ?? (_createWorkCopyCommand = new AsyncCommand(ExecuteCreateWorkCopy));

        private async Task ExecuteCreateWorkCopy()
        {
            string path = input.OpenDirectory(Resources.UserInput.Descriptions.SelectSaveFolder);
            if (path == null) return;

            logbook.Write($"Started work copy creation.", LogLevel.Information);

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            ProgressDialog dialog = new ProgressDialog(string.Empty, tokenSource)
            {
                TotalPercent = 0,
                Phase = Resources.Operations.PhaseNames.Unassigned
            };

            Task showProgress = dialogAssist.Show(dialog);

            IList<FileInfo> created = await executor.Remove(OriginalFiles, new DirectoryInfo(path), tokenSource);
            
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
    }
}
