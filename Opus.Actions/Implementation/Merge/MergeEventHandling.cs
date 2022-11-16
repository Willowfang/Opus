using Opus.Actions.Services.Merge;
using Opus.Common.Wrappers;
using Opus.Events;
using Opus.Common.Logging;
using Prism.Events;
using WF.LoggingLib;

namespace Opus.Actions.Implementation.Merge
{
    /// <summary>
    /// Implementation for <see cref="IMergeEventHandling"/>.
    /// </summary>
    public class MergeEventHandling : LoggingCapable<MergeEventHandling>, IMergeEventHandling
    {
        private IEventAggregator eventAggregator;
        private IMergeProperties properties;

        /// <summary>
        /// When the view associated with this view model is active and showing, subscribe to receive
        /// notifications, when user selects new files for merging. When inactive and not showing,
        /// do not receive notifications. This token is stored to unsubscribe from the correct event.
        /// </summary>
        private SubscriptionToken? filesAddedSubscription;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="logbook">Logging service.</param>
        /// <param name="eventAggregator">Event service.</param>
        /// <param name="properties">Merge properties service.</param>
        public MergeEventHandling(
            IEventAggregator eventAggregator,
            IMergeProperties properties,
            ILogbook logbook) : base(logbook)
        {
            this.eventAggregator = eventAggregator;
            this.properties = properties;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void ActionReset()
        {
            logbook.Write($"Resetting action state.", LogLevel.Debug);

            properties.Collection?.Clear();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void SubscribeToEvents()
        {
            // Subscribe to file addition event.

            logbook.Write($"Subscribing to events.", LogLevel.Information);

            filesAddedSubscription = eventAggregator
                .GetEvent<FilesAddedEvent>()
                .Subscribe(x => FilesAdded(x, properties));

            logbook.Write($"Subscribed to events.", LogLevel.Debug);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void UnsubscribeFromEvents()
        {
            // Unsubscribe from file addition event.

            logbook.Write($"Unsubscribing from events.", LogLevel.Debug);

            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);

            logbook.Write($"{this} unsubscribed from {nameof(FilesAddedEvent)}.", LogLevel.Debug);

            logbook.Write($"Subscriptions cancelled.", LogLevel.Debug);
        }

        /// <summary>
        /// When files are selected, add them to the collection.
        /// </summary>
        /// <param name="files">Paths of the files to add.</param>
        /// <param name="properties">Merge properties service.</param>
        private void FilesAdded(string[] files, IMergeProperties properties)
        {
            logbook.Write($"Handling file addition event.", LogLevel.Debug);

            foreach (string file in files)
            {
                properties.Collection.Add(new FileStorage(file));
            }

            logbook.Write($"File addition event handled.", LogLevel.Debug);
        }
    }
}
