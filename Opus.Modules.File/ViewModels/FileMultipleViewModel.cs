using Microsoft.Win32;
using Opus.Core.Base;
using Opus.Events;
using Opus.Services.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Opus.Modules.File.ViewModels
{
    public class FileMultipleViewModel : ViewModelBase
    {
        private IPathSelection input;
        private IEventAggregator eventAggregator;

        public FileMultipleViewModel(IEventAggregator eventAggregator,
            IPathSelection input) 
        {
            this.input = input;
            this.eventAggregator = eventAggregator;
        }

        private DelegateCommand _addFiles;
        public DelegateCommand AddFiles =>
            _addFiles ?? (_addFiles = new DelegateCommand(ExecuteAddFiles));

        private void ExecuteAddFiles()
        {
            string[] path = input.OpenFiles(Resources.Labels.FileDialogMultiple,
                FileType.PDF);
            if (path.Length == 0) return;
            
            eventAggregator.GetEvent<FilesAddedEvent>().Publish(path);
        }
    }
}
