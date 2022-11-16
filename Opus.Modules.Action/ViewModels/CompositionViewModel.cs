using WF.LoggingLib;
using Opus.Values;
using Opus.Common.Services.Navigation;
using Opus.Actions.Base;
using Opus.Actions.Services.Compose;
using Opus.Common.Services.Commands;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// View model for composition views. Handles actions dealing with document composition.
    /// </summary>
    public class CompositionViewModel : 
        ActionViewModelBase<
            CompositionViewModel,
            ICompositionProperties,
            ICompositionEventHandling,
            ICompositionCommands>,
        INavigationTarget
    {
        /// <summary>
        /// Create a new viewmodel for the composition view.
        /// </summary>
        /// <param name="commands">Compose commands service.</param>
        /// <param name="events">Compose event handling service.</param>
        /// <param name="navigationRegistry">Navigation service.</param>
        /// <param name="properties">Compose properties service.</param>
        /// <param name="logbook">Logging service.</param>
        public CompositionViewModel(
            ICompositionProperties properties,
            ICompositionEventHandling events,
            ICompositionCommands commands,
            INavigationTargetRegistry navigationRegistry,
            ILogbook logbook
        ) : base(properties, events, commands,logbook)
        {
            navigationRegistry.AddTarget(SchemeNames.COMPOSE, this);
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>.
        /// <para>
        /// Actions to take when this viewmodel is navigated to.
        /// </para>
        /// </summary>
        public void OnArrival()
        {
            Events.SubscribeToEvents();
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>.
        /// <para>
        /// Actions to take when this viewmodel is navigated from.
        /// </para>
        /// </summary>
        public void WhenLeaving()
        {
            Events.UnsubscribeFromEvents();
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>.
        /// <para>
        /// Actions to take when this reset button is presseed.
        /// </para>
        /// </summary>
        public void Reset() 
        {
            Events.ActionReset();
        }
    }
}
