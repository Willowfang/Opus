using Opus.Core.Base;
using Opus.Core.Events;
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
using Opus.Core.Dialog;

namespace Opus.Modules.Action.ViewModels
{
    public class SignatureRemovalViewModel : ViewModelBase
    {
        public ObservableCollection<FileStorage> SignatureFiles { get; set; }

        private FileStorage selectedFile;
        public FileStorage SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        private IConfiguration.Sign Configuration;
        private IManipulator Manipulator;

        public SignatureRemovalViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, 
            IConfiguration.Sign conf, IManipulator manipulator)
            : base(regionManager, eventAggregator)
        {
            Aggregator.GetEvent<FilesAddedEvent>().Subscribe(FilesAdded);
            SignatureFiles = new ObservableCollection<FileStorage>();
            Configuration = conf;
            Manipulator = manipulator;
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
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.Description = Resources.Labels.Bookmarks_SelectFolder;
            browseDialog.UseDescriptionForTitle = true;
            browseDialog.ShowNewFolderButton = true;

            if (browseDialog.ShowDialog() == DialogResult.Cancel)
                return;

            await Manipulator.RemoveSignatureAsync(SignatureFiles.Where(x => x.IsSelected).Select(y => y.FilePath).ToArray(),
                new System.IO.DirectoryInfo(browseDialog.SelectedPath), Configuration.SignatureRemovePostfix);
            Aggregator.GetEvent<ShowDialogEvent>().Publish(new MessageDialog(Resources.DialogMessages.SignatureRemoved));
        }
    }
}
