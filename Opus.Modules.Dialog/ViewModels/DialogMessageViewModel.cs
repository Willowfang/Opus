using Opus.Core.Base;
using Opus.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opus.Modules.Dialog.ViewModels
{
    public class DialogMessageViewModel : ViewModelBase
    {
        private string message;
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public DialogMessageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
            : base(regionManager, eventAggregator)
        {
            Aggregator.GetEvent<DialogMessageEvent>().Subscribe(x => Message = x);
        }
    }
}
