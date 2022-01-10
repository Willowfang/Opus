using Opus.Core.Base;
using Opus.Core.ExtensionMethods;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Opus.Services.Configuration;
using CX.PdfLib.Services;
using Opus.Core.Wrappers;
using AsyncAwaitBestPractices.MVVM;
using System.Threading.Tasks;
using Opus.Core.Constants;
using Opus.Services.Input;
using Opus.Services.UI;
using Opus.Events;
using CX.PdfLib.Common;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Data;
using Opus.Services.Extensions;
using System.Threading;

namespace Opus.Modules.Action.ViewModels
{
    public class SignatureRemovalViewModel : ViewModelBase, INavigationTarget
    {
        // Signature-related configuration service
        private ISignatureOptions options;
        // General PDF-manipulator service
        private IManipulator manipulator;
        // Service for getting user input related to file paths
        private IPathSelection input;
        // Prism events service
        private IEventAggregator eventAggregator;
        /// <summary>
        /// Common service for displaying and updating a dialog
        /// </summary>
        private IDialogAssist dialogAssist;

        // Collection for files that will have their signatures removed
        public ObservableCollection<FileStorage> SignatureFiles { get; set; }
        private FileStorage selectedFile;
        // Currently selected file
        public FileStorage SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        public SignatureRemovalViewModel(IEventAggregator eventAggregator, 
            ISignatureOptions options, IManipulator manipulator, IPathSelection input,
            INavigationTargetRegistry navRegistry, IDialogAssist dialogAssist)
        {
            SignatureFiles = new ObservableCollection<FileStorage>();
            this.options = options;
            this.manipulator = manipulator;
            this.input = input;
            this.eventAggregator = eventAggregator;
            this.dialogAssist = dialogAssist;
            navRegistry.AddTarget(SchemeNames.SIGNATURE, this);
        }

        SubscriptionToken filesAddedSubscription;
        public void OnArrival()
        {
            filesAddedSubscription = eventAggregator.GetEvent<FilesAddedEvent>().Subscribe(FilesAdded);
        }
        public void WhenLeaving()
        {
            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);
        }

        private void FilesAdded(string[] addedFiles)
        {
            foreach (string file in addedFiles)
            {
                SignatureFiles.Add(new FileStorage(file));
            }
        }

        private DelegateCommand _deleteCommand;
        public DelegateCommand DeleteCommand =>
            _deleteCommand ?? (_deleteCommand = new DelegateCommand(ExecuteDeleteCommand));

        private void ExecuteDeleteCommand()
        {
            SignatureFiles.RemoveAll(x => x.IsSelected);
        }

        private DelegateCommand _clearCommand;
        public DelegateCommand ClearCommand =>
            _clearCommand ?? (_clearCommand = new DelegateCommand(ExecuteClearCommand));

        private void ExecuteClearCommand()
        {
            SignatureFiles.Clear();
        }

        private IAsyncCommand _removeSignatureCommand;
        public IAsyncCommand RemoveSignatureCommand =>
            _removeSignatureCommand ?? (_removeSignatureCommand = new AsyncCommand(ExecuteRemoveSignatureCommand));

        private async Task ExecuteRemoveSignatureCommand()
        {
            string path = input.OpenDirectory(Resources.UserInput.Descriptions.SelectSaveFolder);
            if (path == null) return;

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            ProgressDialog progressDialog = new ProgressDialog(null, tokenSource)
            {
                TotalPercent = 0,
                Phase = ProgressPhase.Unassigned.GetResourceString()
            };

            Task progressTask = dialogAssist.Show(progressDialog);
            await manipulator.RemoveSignatureAsync(SignatureFiles.Select(y => y.FilePath).ToArray(),
                new System.IO.DirectoryInfo(path), options.Suffix);
            progressDialog.TotalPercent = 100;
            progressDialog.Phase = ProgressPhase.Finished.GetResourceString();
            await progressTask;
        }
    }
}
