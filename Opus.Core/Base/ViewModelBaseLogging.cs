using CX.LoggingLib;
using Opus.Services.Implementation.Logging;
using Prism.Mvvm;
using Prism.Regions;

namespace Opus.Core.Base
{
    public abstract class ViewModelBaseLogging<T> : LoggingCapable<T>, INavigationAware
    {
        public ViewModelBaseLogging(ILogbook logbook) : base(logbook) { }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
            => true;

        public virtual void OnNavigatedFrom(NavigationContext navigationContext) { }

        public virtual void OnNavigatedTo(NavigationContext navigationContext) { }
    }

    public abstract class ViewModelBase : BindableBase, INavigationAware
    {
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
            => true;

        public virtual void OnNavigatedFrom(NavigationContext navigationContext) { }

        public virtual void OnNavigatedTo(NavigationContext navigationContext) { }
    }
}
