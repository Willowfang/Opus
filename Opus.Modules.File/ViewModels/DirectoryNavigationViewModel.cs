using Opus.Core.Base;
using Opus.Events;
using Opus.Services.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Opus.Modules.File.ViewModels
{
    public class DirectoryNavigationViewModel : ViewModelBase
    {
        private IPathSelection input;
        private IEventAggregator eventAggregator;

        private DirectoryInfo directory;
        public string DirectoryName
        {
            get
            {
                if (directory == null)
                    return Resources.Buttons.General.SelectFolder;

                return directory.Name;
            }
            set => SetProperty(ref directory, new DirectoryInfo(value));
        }

        public DirectoryNavigationViewModel(IPathSelection input, IEventAggregator eventAggregator)
        {
            this.input = input;
            this.eventAggregator = eventAggregator;
        }

        private DelegateCommand openDirectory;
        public DelegateCommand OpenDirectory =>
            openDirectory ??= new DelegateCommand(ExecuteOpenDirectory);

        private void ExecuteOpenDirectory()
        {
            string path = input.OpenDirectory(Resources.UserInput.Descriptions.SelectOpenFolder);
            if (path == null) return;
            DirectoryName = path;
            eventAggregator.GetEvent<DirectorySelectedEvent>().Publish(path);
        }
    }
}
