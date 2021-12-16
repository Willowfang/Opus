using Opus.Core.Base;
using Opus.Services.Configuration;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opus.Modules.Options.ViewModels
{
    public class MergeOptionsViewModel : ViewModelBase
    {
        private IConfiguration.Merge configuration;

        private bool addPages;
        public bool AddPages
        {
            get => addPages;
            set
            {
                configuration.AddPageNumbers = value;
                SetProperty(ref addPages, value);
            }
        }
        public MergeOptionsViewModel(IConfiguration.Merge configuration)
        {
            this.configuration = configuration;
            addPages = configuration.AddPageNumbers;
        }
    }
}
