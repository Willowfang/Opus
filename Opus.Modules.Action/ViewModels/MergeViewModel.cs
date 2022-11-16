using WF.LoggingLib;
using Opus.Values;
using Opus.Common.Services.Navigation;
using Opus.Actions.Services.Merge;
using Opus.Actions.Base;
using Opus.Common.Services.Commands;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// A viewModel for actions for merging documents.
    /// </summary>
    public class MergeViewModel : 
        ActionViewModelBase<
            MergeViewModel,
            IMergeProperties,
            IMergeEventHandling,
            IMergeCommands>, INavigationTarget
    {

        /// <summary>
        /// Create a new Merge View Model.
        /// </summary>
        /// <param name="navRegistry">Navigation registry service.</param>
        /// <param name="commands">Merge action commands service.</param>
        /// <param name="events">Merge event handling service.</param>
        /// <param name="properties">Merge properties service.</param>
        /// <param name="logbook">Logging services.</param>
        public MergeViewModel(
            INavigationTargetRegistry navRegistry,
            IMergeProperties properties,
            IMergeEventHandling events,
            IMergeCommands commands,
            ILogbook logbook
        ) : base(properties, events, commands, logbook)
        {
            // Register this viewModel in the navigation registry with the correct scheme.
            navRegistry.AddTarget(SchemeNames.MERGE, this);
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
        /// Implementing <see cref="INavigationTarget"/>.
        /// <para>
        /// When reset button is pressed, clear the <see cref="IMergeProperties.Collection"/>.
        /// </para>
        /// </summary>
        public void Reset()
        {
            Events.ActionReset();
        }
    }
}
