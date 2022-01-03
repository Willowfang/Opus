using Opus.Core.Base;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opus.Modules.Dialog.ViewModels
{
    public class DialogContentViewModel : ViewModelBase
    {
        public DialogContentViewModel(IRegionManager manager, IEventAggregator aggregator)
            : base(manager, aggregator)
        {

        }
    }
}
