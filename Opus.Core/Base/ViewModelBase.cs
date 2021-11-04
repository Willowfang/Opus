using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Opus.Core.Base
{
    public class ViewModelBase : BindableBase, INavigationAware
    {
        protected IRegionManager RegionManager { get; }
        protected IEventAggregator Aggregator { get; }

        public ViewModelBase(IRegionManager regionManager, IEventAggregator aggregator)
        {
            RegionManager = regionManager;
            Aggregator = aggregator;
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
    }
}
