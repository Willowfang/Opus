using Opus.Values;
using WF.LoggingLib;
using Opus.Actions.Services.Extract;
using Opus.Common.Services.Commands;
using Opus.Actions.Base;
using Opus.Common.Services.Navigation;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// ViewModel dealing with files and bookmarks up for extraction.
    /// </summary>
    public class ExtractionViewModel : 
        ActionViewModelBase<
            ExtractionViewModel, 
            IExtractionActionProperties,
            IExtractionActionEventHandling,
            IExtractionActionCommands>, 
        INavigationTarget
    {
        /// <summary>
        /// ViewModel for handling selection of extractable bookmarks in files.
        /// </summary>
        /// <param name="navregistry">Navigation registry for viewModels.</param>
        /// <param name="properties">Extraction properties.</param>
        /// <param name="events">Extraction event handling.</param>
        /// <param name="commands">Extraction commands.</param>
        /// <param name="logbook">Loggin service.</param>
        public ExtractionViewModel(
            INavigationTargetRegistry navregistry,
            IExtractionActionProperties properties,
            IExtractionActionEventHandling events,
            IExtractionActionCommands commands,
            ILogbook logbook
        ) : base(properties, events, commands, logbook)
        {
            // Assign services and associate this viewmodel with
            // a navigation scheme.

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
