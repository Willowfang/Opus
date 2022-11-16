using WF.LoggingLib;
using Opus.Common.Services.Navigation;
using Opus.Values;
using Opus.Actions.Services.Extract;
using Opus.Common.Services.Commands;
using Opus.Actions.Base;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// ViewModel for organizing bookmarks for extraction. Depends on more general extraction services.
    /// Communicates (via events) with <see cref="ExtractionViewModel"/>.
    /// </summary>
    public class ExtractionOrderViewModel : 
        ActionViewModelBase<
            ExtractionOrderViewModel,
            IExtractionSupportProperties,
            IExtractionSupportEventHandling,
            IExtractionSupportCommands>,
        INavigationTarget
    {
        /// <summary>
        /// ViewModel for extraction bookmark ordering.
        /// </summary>
        /// <remarks>Parameters are received through dependency injection</remarks>
        /// <param name="navregistry">Navigation registry for viewModels.</param>
        /// <param name="commands">Extraction support section commands service.</param>
        /// <param name="events">Extraction support section event handling service.</param>
        /// <param name="properties">Extraction support section properties.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionOrderViewModel(
            INavigationTargetRegistry navregistry,
            IExtractionSupportProperties properties,
            IExtractionSupportEventHandling events,
            IExtractionSupportCommands commands,
            ILogbook logbook
        ) : base(properties, events, commands, logbook)
        {
            navregistry.AddTarget(SchemeNames.EXTRACT, this);
        }

        /// <summary>
        /// See <see cref="INavigationTarget"/>, <see cref="INavigationTargetRegistry"/> and
        /// <see cref="INavigationAssist"/> for more information.
        /// </summary>
        public void OnArrival()
        {
            Events.SubscribeToEvents();
        }

        /// <summary>
        /// See <see cref="INavigationTarget"/>, <see cref="INavigationTargetRegistry"/> and
        /// <see cref="INavigationAssist"/> for more information.
        /// </summary>
        public void WhenLeaving()
        {
            Events.UnsubscribeFromEvents();
        }

        /// <summary>
        /// See <see cref="INavigationTarget"/>, <see cref="INavigationTargetRegistry"/> and
        /// <see cref="INavigationAssist"/> for more information.
        /// </summary>
        public void Reset()
        {
            Events.ActionReset();
        }
    }
}
