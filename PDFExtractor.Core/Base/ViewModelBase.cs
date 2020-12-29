using Prism.Mvvm;
using Prism.Regions;

namespace PDFExtractor.Core.Base
{
    public class ViewModelBase : BindableBase, INavigationAware
    {
        protected IRegionManager RegionManager { get; }

        public ViewModelBase(IRegionManager regionManager)
        {
            RegionManager = regionManager;
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
