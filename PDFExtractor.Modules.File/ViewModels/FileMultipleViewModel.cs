using Microsoft.Win32;
using PDFExtractor.Core.Base;
using PDFExtractor.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.IO;

namespace PDFExtractor.Modules.File.ViewModels
{
    public class FileMultipleViewModel : ViewModelBase
    {
        public FileMultipleViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) 
            : base(regionManager, eventAggregator) { }

        private DelegateCommand _addFiles;
        public DelegateCommand AddFiles =>
            _addFiles ?? (_addFiles = new DelegateCommand(ExecuteAddFiles));

        private void ExecuteAddFiles()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = Resources.Labels.FileDialogMultiple;
            openFile.Filter = "PDF |*.pdf";
            openFile.Multiselect = true;

            if (openFile.ShowDialog() != true)
                return;

            Aggregator.GetEvent<FilesAddedEvent>().Publish(openFile.FileNames);
        }
    }
}
