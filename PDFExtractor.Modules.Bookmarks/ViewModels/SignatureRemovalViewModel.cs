using ExtLib;
using PDFExtractor.Core.Base;
using PDFExtractor.Core.Events;
using PDFExtractor.Core.ExtensionMethods;
using PDFExtractor.Core.Singletons;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace PDFExtractor.Modules.Bookmarks.ViewModels
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

        private ICommonValues valueSingleton { get; set; }

        public SignatureRemovalViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ICommonValues commonValues)
            : base(regionManager, eventAggregator)
        {
            Aggregator.GetEvent<FilesAddedEvent>().Subscribe(FilesAdded);
            SignatureFiles = new ObservableCollection<IFilePDF>();
            valueSingleton = commonValues;
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

            Signature.Remove(SignatureFiles.ToArray(), browseDialog.SelectedPath, valueSingleton.Identifier);
            Aggregator.GetEvent<DialogMessageEvent>().Publish(Resources.DialogMessages.Bookmarks_MultipleSaved);
        }
    }
}
