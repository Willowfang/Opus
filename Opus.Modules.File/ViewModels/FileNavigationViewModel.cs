using Microsoft.Win32;
using Opus.Core.Base;
using Opus.Core.Constants;
using Opus.Events;
using Opus.Services.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.IO;

namespace Opus.Modules.File.ViewModels
{
    public class FileNavigationViewModel : ViewModelBase
    {
        private IPathSelection input;
        private IEventAggregator eventAggregator;

        private string fileName;
        public string FileName
        {
            get { return Path.GetFileNameWithoutExtension(fileName); }
            set { SetProperty(ref fileName, value); }
        }

        public FileNavigationViewModel(IEventAggregator eventAggregator,
            IPathSelection input) 
        {
            FileName = Resources.Labels.FileButtonSingle;
            this.input = input;
            this.eventAggregator = eventAggregator;
        }

        private DelegateCommand _openFile;
        public DelegateCommand OpenFile =>
            _openFile ?? (_openFile = new DelegateCommand(ExecuteOpenFile));

        void ExecuteOpenFile()
        {
            string path = input.OpenFile(Resources.Labels.FileDialogSingle,
                FileType.PDF);
            if (path == null) return;

            FileName = path;
            eventAggregator.GetEvent<FileSelectedEvent>().Publish(path);
        }
    }
}
