using Microsoft.Win32;
using PDFExtractor.Core;
using PDFExtractor.Core.Base;
using PDFExtractor.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PDFExtractor.Modules.File.ViewModels
{
    public class FileNavigationViewModel : ViewModelBase
    {
        private IEventAggregator aggregator;

        private string fileName;
        public string FileName
        {
            get { return Path.GetFileNameWithoutExtension(fileName); }
            set { SetProperty(ref fileName, value); }
        }

        public FileNavigationViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(regionManager)
        {
            aggregator = eventAggregator;
            FileName = "Avaa PDF";
        }

        private DelegateCommand _openFile;
        public DelegateCommand OpenFile =>
            _openFile ?? (_openFile = new DelegateCommand(ExecuteOpenFile));

        void ExecuteOpenFile()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Valitse tiedosto";
            openDialog.Filter = "PDF |*.pdf";

            if (openDialog.ShowDialog() != true)
                return;

            FileName = openDialog.FileName;
            aggregator.GetEvent<FileSelectedEvent>().Publish(fileName);
        }
    }
}
