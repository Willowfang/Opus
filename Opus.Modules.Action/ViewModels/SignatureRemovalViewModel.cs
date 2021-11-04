using PDFLib.Implementation;
using PDFLib.Services;
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

namespace Opus.Modules.Action.ViewModels
{
    public class SignatureRemovalViewModel : ViewModelBase
    {
        public ObservableCollection<IFilePDF> SignatureFiles { get; set; }

        private IFilePDF selectedFile;
        public IFilePDF SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        private IConfiguration.Sign Configuration;
        private ISignature Signature;

        public SignatureRemovalViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, 
            IConfiguration.Sign conf, ISignature signature)
            : base(regionManager, eventAggregator)
        {
            Aggregator.GetEvent<FilesAddedEvent>().Subscribe(FilesAdded);
            SignatureFiles = new ObservableCollection<IFilePDF>();
            Configuration = conf;
            Signature = signature;
        }

        private void FilesAdded(string[] addedFiles)
        {
            foreach (string file in addedFiles)
            {
                SignatureFiles.Add(new FilePDF(file));
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

        private DelegateCommand _removeSignatureCommand;
        public DelegateCommand RemoveSignatureCommand =>
            _removeSignatureCommand ?? (_removeSignatureCommand = new DelegateCommand(ExecuteRemoveSignatureCommand));

        private void ExecuteRemoveSignatureCommand()
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.Description = Resources.Labels.Bookmarks_SelectFolder;
            browseDialog.UseDescriptionForTitle = true;
            browseDialog.ShowNewFolderButton = true;

            if (browseDialog.ShowDialog() == DialogResult.Cancel)
                return;

            Signature.Remove(SignatureFiles.ToArray(), browseDialog.SelectedPath, Configuration.SignatureRemovePostfix);
            Aggregator.GetEvent<DialogMessageEvent>().Publish(Resources.DialogMessages.Bookmarks_MultipleSaved);
        }
    }
}
