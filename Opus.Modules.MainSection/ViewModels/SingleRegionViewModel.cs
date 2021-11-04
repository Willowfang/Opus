using Opus.Core.Base;
using Prism.Events;
using Prism.Regions;

namespace Opus.Modules.MainSection.ViewModels
{
    public class SingleRegionViewModel : ViewModelBase
    {
        public SingleRegionViewModel(IRegionManager manager, IEventAggregator aggregator) :
            base(manager, aggregator) { }
    }
}
