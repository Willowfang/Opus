using Opus.Actions.Services.WorkCopy;
using Opus.Common.Wrappers;
using Opus.Events;
using Opus.Common.Logging;
using Prism.Events;
using WF.LoggingLib;

namespace Opus.Actions.Implementation.WorkCopy
{
    /// <summary>
    /// Implementation for <see cref="IWorkCopyEventHandling"/>.
    /// </summary>
    public class WorkCopyEventHandling : LoggingCapable<WorkCopyEventHandling>, IWorkCopyEventHandling
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IWorkCopyProperties properties;

        /// <summary>
        /// Subscription token for filesaddedevent. Stored for unsubscribing when leaving this viewmodel.
        /// </summary>
        private SubscriptionToken? filesAddedSubscription;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="properties">Work copy properties service.</param>
        /// <param name="eventAggregator">Event service.</param>
        /// <param name="logbook">Logging service.</param>
        public WorkCopyEventHandling(
            IWorkCopyProperties properties,
            IEventAggregator eventAggregator,
            ILogbook logbook) : base(logbook)
        {
            this.properties = properties;
            this.eventAggregator = eventAggregator;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void ActionReset()
        {
            logbook.Write($"Resetting action state.", LogLevel.Debug);

            properties.OriginalFiles?.Clear();
            properties.SelectedFile = null;

            logbook.Write($"Action state reset.", LogLevel.Debug);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void SubscribeToEvents()
        {
            logbook.Write($"Subscribing to events.", LogLevel.Debug);

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
            logbook.Write($"Unsubscribing from events.", LogLevel.Debug);

            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);

            logbook.Write($"Subscriptions cancelled.", LogLevel.Debug);
        }

        private void FilesAdded(string[] addedFiles, IWorkCopyProperties properties)
        {
            logbook.Write($"Handling file addition event.", LogLevel.Debug);

            foreach (string file in addedFiles)
            {
                if (!properties.OriginalFiles.Any(f => f.FilePath == file))
                    properties.OriginalFiles.Add(new FileStorage(file));
            }

            logbook.Write($"File addition event handled.", LogLevel.Debug);
        }
    }
}
