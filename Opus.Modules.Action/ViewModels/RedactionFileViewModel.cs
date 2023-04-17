using Opus.Actions.Base;
using Opus.Actions.Services.Redact;
using Opus.Common.Services.Commands;
using Opus.Common.Services.Navigation;
using Opus.Values;
using WF.LoggingLib;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// A viewmodel for holding information about redactable files.
    /// </summary>
    public class RedactionFileViewModel : 
        ActionViewModelBase<
            RedactionFileViewModel,
            IRedactProperties,
            IRedactEventHandling,
            IRedactCommands>, INavigationTarget
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="properties"></param>
        /// <param name="eventHandling"></param>
        /// <param name="commands"></param>
        /// <param name="logbook"></param>
        public RedactionFileViewModel(
            INavigationTargetRegistry registry,
            IRedactProperties properties,
            IRedactEventHandling eventHandling,
            IRedactCommands commands,
            ILogbook logbook)
            : base(properties, eventHandling, commands, logbook)
        {
            registry.AddTarget(SchemeNames.REDACT, this);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnArrival()
        {
            Events.SubscribeToEvents();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Reset()
        {
            Events.ActionReset();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void WhenLeaving()
        {
            Events.UnsubscribeFromEvents();
        }
    }
}
