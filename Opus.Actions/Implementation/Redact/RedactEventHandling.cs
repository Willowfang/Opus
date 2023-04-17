using Opus.Actions.Services.Merge;
using Opus.Actions.Services.Redact;
using Opus.Common.Logging;
using Opus.Common.Wrappers;
using Opus.Events;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF.LoggingLib;

namespace Opus.Actions.Implementation.Redact
{
    /// <summary>
    /// Class handling events for redaction.
    /// </summary>
    public class RedactEventHandling : LoggingCapable<RedactEventHandling>, IRedactEventHandling
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IRedactProperties properties;

        /// <summary>
        /// When the view associated with this view model is active and showing, subscribe to receive
        /// notifications, when user selects new files for redaction. When inactive and not showing,
        /// do not receive notifications. This token is stored to unsubscribe from the correct event.
        /// </summary>
        private SubscriptionToken? filesAddedSubscription;

        /// <summary>
        /// Create an instance handling events for redaction actions.
        /// </summary>
        /// <param name="eventAggregator">Event service.</param>
        /// <param name="properties">Redaction properties.</param>
        /// <param name="logbook">Logging service.</param>
        public RedactEventHandling(
            IEventAggregator eventAggregator, 
            IRedactProperties properties,
            ILogbook logbook) : base(logbook)
        {
            this.eventAggregator = eventAggregator;
            this.properties = properties;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ActionReset()
        {
            logbook.Write($"Resetting action state.", LogLevel.Debug);

            properties.Files.Clear();
            properties.WordsToRedact = null;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
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
        /// <param name="properties">Redact properties service.</param>
        private void FilesAdded(string[] files, IRedactProperties properties)
        {
            logbook.Write($"Handling file addition event.", LogLevel.Debug);

            foreach (string file in files)
            {
                properties.Files.Add(new FileStorage(file));
            }

            logbook.Write($"File addition event handled.", LogLevel.Debug);
        }
    }
}
