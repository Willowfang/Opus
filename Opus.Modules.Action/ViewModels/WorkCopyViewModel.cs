using Opus.Values;
using Opus.Common.Services.Navigation;
using WF.LoggingLib;
using Opus.Actions.Base;
using Opus.Actions.Services.WorkCopy;
using Opus.Common.Services.Commands;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// ViewModel for creating work copies of pdf-files.
    /// </summary>
    public class WorkCopyViewModel : 
        ActionViewModelBase<
            WorkCopyViewModel,
            IWorkCopyProperties,
            IWorkCopyEventHandling,
            IWorkCopyCommands>, 
        INavigationTarget
    {
        /// <summary>
        /// Create a new viewModel for handling work copy creation.
        /// </summary>
        /// <param name="navRegistry">Navigation registry for viewModels.</param>
        /// <param name="commands">Work copy commands service.</param>
        /// <param name="events">Work copy event handling service.</param>
        /// <param name="properties">Work copy properties.</param>
        /// <param name="logbook">Logging services.</param>
        public WorkCopyViewModel(
            IWorkCopyProperties properties,
            IWorkCopyEventHandling events,
            IWorkCopyCommands commands,
            INavigationTargetRegistry navRegistry,
            ILogbook logbook
        ) : base(properties, events, commands, logbook)
        {
            // Add this viewmodel as navigation target with proper scheme.

            navRegistry.AddTarget(SchemeNames.WORKCOPY, this);
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>. Subscribe to events.
        /// </summary>
        public void OnArrival()
        {
            Events.SubscribeToEvents();
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>. Unsubscribe from events.
        /// </summary>
        public void WhenLeaving()
        {
            Events.UnsubscribeFromEvents();
        }

        /// <summary>
        /// When reset is requested by the user, clear files list and deselect any files.
        /// </summary>
        public void Reset()
        {
            Events.ActionReset();
        }
    }
}
