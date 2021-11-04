using Microsoft.Win32;
using Opus.Core.Base;
using Opus.Core.Constants;
using Opus.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.IO;

namespace Opus.Modules.File.ViewModels
{
    public class FileNavigationViewModel : ViewModelBase
    {
        private string fileName;
        public string FileName
        {
            get { return Path.GetFileNameWithoutExtension(fileName); }
            set { SetProperty(ref fileName, value); }
        }

        public FileNavigationViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) 
            : base(regionManager, eventAggregator)
        {
            FileName = Resources.Labels.FileButtonSingle;
        }

        private DelegateCommand _openFile;
        public DelegateCommand OpenFile =>
            _openFile ?? (_openFile = new DelegateCommand(ExecuteOpenFile));

        void ExecuteOpenFile()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = Resources.Labels.FileDialogSingle;
            openDialog.Filter = "PDF |*.pdf";

            if (openDialog.ShowDialog() != true)
                return;

            FileName = openDialog.FileName;
            Aggregator.GetEvent<FileSelectedEvent>().Publish(fileName);
        }
    }
}
