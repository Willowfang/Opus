using Opus.Actions.Services.Extract;
using Opus.Events;
using Opus.Common.Logging;
using Prism.Events;
using WF.LoggingLib;

namespace Opus.Actions.Implementation.Extract
{
    /// <summary>
    /// Event handling methods for extraction action.
    /// </summary>
    public class ExtractionActionEventHandling : LoggingCapable<ExtractionActionEventHandling>, IExtractionActionEventHandling
    {
        private IEventAggregator eventAggregator;
        private IExtractionActionProperties properties;
        private IExtractionActionMethods methods;

        // Hold tokens for events that have been subscribed to (in order to unsubscribe when leaving).
        internal SubscriptionToken? filesAddedSubscription;
        internal SubscriptionToken? bookmarkDeselectedSubscription;

        /// <summary>
        /// Create a new instance for handling extract action events.
        /// </summary>
        /// <param name="eventAggregator">Event service.</param>
        /// <param name="properties">Extraction properties.</param>
        /// <param name="methods">Extraction methods service.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionActionEventHandling(
            IEventAggregator eventAggregator,
            IExtractionActionProperties properties,
            IExtractionActionMethods methods,
            ILogbook logbook) : base(logbook)
        {
            this.eventAggregator = eventAggregator;
            this.properties = properties;
            this.methods = methods;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SubscribeToEvents()
        {
            // This action needs to know when the user has selected new files and if some bookmarks have been
            // unselected (in order to redisplay them with this action).

            // Subscribe to events that inform of the abovementioned cases.

            logbook.Write($"Subscribing to events.", LogLevel.Debug);

            filesAddedSubscription = eventAggregator
                .GetEvent<FilesAddedEvent>()
                .Subscribe(FilesAddedHandler);

            bookmarkDeselectedSubscription = eventAggregator
                .GetEvent<BookmarkDeselectedEvent>()
                .Subscribe(BookmarkDeselectedHandler);

            logbook.Write($"Subscription completed.", LogLevel.Debug);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            // Unsubscribe from file and unselection notifications.

            logbook.Write($"Unsubscribing to events.", LogLevel.Debug);

            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);

            eventAggregator.GetEvent<BookmarkDeselectedEvent>()
                .Unsubscribe(bookmarkDeselectedSubscription);

            logbook.Write($"Unsubscribed from events.", LogLevel.Debug);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ActionReset()
        {
            logbook.Write($"Resetting action state.", LogLevel.Information);

            properties.Files.Clear();
            properties.FileBookmarks?.Clear();
            properties.SelectedFile = null;
        }

        /// <summary>
        /// Handler for dealing with file additions (through file addition event).
        /// </summary>
        /// <param name="filePaths">Filepaths of the added files.</param>
        private async void FilesAddedHandler(string[] filePaths)
        {
            await methods.AddNewFiles(filePaths);
        }

        /// <summary>
        /// Handler for dealing with deselection of bookmarks.
        /// </summary>
        /// <param name="id">Id of the deselected bookmark.</param>
        private void BookmarkDeselectedHandler(Guid id)
        {
            methods.DeselectBookmark(id);
        }
    }
}
