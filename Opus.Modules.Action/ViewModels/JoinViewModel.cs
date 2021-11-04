using Opus.Core.Base;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opus.Modules.Action.ViewModels
{
    public class JoinViewModel : ViewModelBase
    {
        public JoinViewModel(IRegionManager manager, IEventAggregator aggregator)
            : base(manager, aggregator)
        {

        }
    }
}
