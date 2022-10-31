using WF.LoggingLib;
using Opus.Services.Implementation.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System.ComponentModel;

namespace Opus.Core.Base
{
    /// <summary>
    /// A viewModel base class with logging capabilities. Widely used as a base class for ViewModels of
    /// this application.
    /// </summary>
    /// <typeparam name="ViewModelType">Type of the viewModel that implements this base class (for logging the
    /// type correctly).</typeparam>
    public abstract class ViewModelBaseLogging<ViewModelType>
        : LoggingCapable<ViewModelType>,
            INavigationAware
    {
        /// <summary>
        /// Create a new viewModel based on this class.
        /// </summary>
        /// <param name="logbook">Instance of the logbook to use.</param>
        public ViewModelBaseLogging(ILogbook logbook) : base(logbook) { }

        /// <summary>
        /// This viewmodel can handle navigation requests. Always return true.
        /// </summary>
        /// <param name="navigationContext">Context of the navigation.</param>
        /// <returns>True</returns>
        public virtual bool IsNavigationTarget(NavigationContext navigationContext) => true;

        /// <summary>
        /// Because all viewmodels based on this class implement <see cref="INavigationAware"/>, they must also implement
        /// <see cref="OnNavigatedFrom(NavigationContext)"/> and <see cref="OnNavigatedTo(NavigationContext)"/>. The
        /// default is an empty method, but this may be overridden in the inheriting class.
        /// </summary>
        /// <param name="navigationContext">Context of the navigation</param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext) { }

        /// <summary>
        /// Because all viewmodels based on this class implement <see cref="INavigationAware"/>, they must also implement
        /// <see cref="OnNavigatedFrom(NavigationContext)"/> and <see cref="OnNavigatedTo(NavigationContext)"/>. The
        /// default is an empty method, but this may be overridden in the inheriting class.
        /// </summary>
        /// <param name="navigationContext">Context of the navigation</param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext) { }
    }

    /// <summary>
    /// The base class for viewmodels without logging capabilities. Implements
    /// <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public abstract class ViewModelBase : BindableBase, INavigationAware
    {
        /// <summary>
        /// This viewmodel can handle navigation requests. Always return true.
        /// </summary>
        /// <param name="navigationContext">Context of the navigation.</param>
        /// <returns>True</returns>
        public virtual bool IsNavigationTarget(NavigationContext navigationContext) => true;

        /// <summary>
        /// Because all viewmodels based on this class implement <see cref="INavigationAware"/>, they must also implement
        /// <see cref="OnNavigatedFrom(NavigationContext)"/> and <see cref="OnNavigatedTo(NavigationContext)"/>. The
        /// default is an empty method, but this may be overridden in the inheriting class.
        /// </summary>
        /// <param name="navigationContext">Context of the navigation</param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext) { }

        /// <summary>
        /// Because all viewmodels based on this class implement <see cref="INavigationAware"/>, they must also implement
        /// <see cref="OnNavigatedFrom(NavigationContext)"/> and <see cref="OnNavigatedTo(NavigationContext)"/>. The
        /// default is an empty method, but this may be overridden in the inheriting class.
        /// </summary>
        /// <param name="navigationContext">Context of the navigation</param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext) { }
    }
}
