using Opus.Actions.Services.Compose;
using Opus.Events;
using Opus.Common.Logging;
using Prism.Events;
using WF.LoggingLib;

namespace Opus.Actions.Implementation.Compose
{
    /// <summary>
    /// Implementation for <see cref="ICompositionEventHandling"/>.
    /// </summary>
    public class CompositionEventHandling : LoggingCapable<CompositionEventHandling>, ICompositionEventHandling
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ICompositionMethods methods;
        private readonly ICompositionProperties properties;

        /// <summary>
        /// When this action is active and shown, subscribe to receive
        /// notifications of directory selection. This token is stored to cancel said description.
        /// </summary>
        private SubscriptionToken? directorySelectedSubscription;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="eventAggregator">Event service.</param>
        /// <param name="methods">Composition methods service.</param>
        /// <param name="properties">Composition properties service.</param>
        /// <param name="logbook">Logging service.</param>
        public CompositionEventHandling(
            IEventAggregator eventAggregator,
            ICompositionMethods methods,
            ICompositionProperties properties,
            ILogbook logbook) : base(logbook)
        {
            this.eventAggregator = eventAggregator;
            this.properties = properties;
            this.methods = methods;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ActionReset()
        {
            // Do nothing. There is no reset process for this action.
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SubscribeToEvents()
        {
            // Subscribe to directory selection events (to start composing).

            logbook.Write($"Subscribing to events.", LogLevel.Debug);

            directorySelectedSubscription = eventAggregator
                .GetEvent<DirectorySelectedEvent>()
                .Subscribe(DirectorySelected);

            logbook.Write($"Subscription completed.", LogLevel.Debug);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            // Unsubscribe from directory selection event.

            logbook.Write($"Unsubscribing from events.", LogLevel.Debug);

            eventAggregator
                .GetEvent<DirectorySelectedEvent>()
                .Unsubscribe(directorySelectedSubscription);

            logbook.Write($"Unsubscribing completed.", LogLevel.Debug);
        }

        /// <summary>
        /// Action to execute when a directory is selected.
        /// <para>
        /// Will immediately run composition.
        /// </para>
        /// </summary>
        /// <param name="path">Path of the selected directory.</param>
        private async void DirectorySelected(string path)
        {
            logbook.Write($"Directory was selected - executing composition.", LogLevel.Debug);

            await methods.ExecuteComposition(path);
        }
    }
}
