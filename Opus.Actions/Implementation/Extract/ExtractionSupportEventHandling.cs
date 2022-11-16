using Opus.Actions.Services.Extract;
using Opus.Events;
using Opus.Events.Data;
using Opus.Common.Extensions;
using Opus.Common.Wrappers;
using Opus.Common.Logging;
using Opus.Common.Collections;
using Prism.Events;
using WF.LoggingLib;
using WF.PdfLib.Common;

namespace Opus.Actions.Implementation.Extract
{
    /// <summary>
    /// Implementation for <see cref="IExtractionSupportEventHandling"/>.
    /// </summary>
    public class ExtractionSupportEventHandling : LoggingCapable<ExtractionSupportEventHandling>,
        IExtractionSupportEventHandling
    {
        private readonly IExtractionSupportProperties properties;
        private readonly IExtractionSupportMethods methods;
        private readonly IEventAggregator eventAggregator;

        // Tokens for holding event subscriptions. Subscriptions will be canceled when the user navigates away.
        private SubscriptionToken? bookmarkEventSubscription;
        private SubscriptionToken? bookmarkFileDeletedEventSubscription;

        /// <summary>
        /// Create a new implementation instance.
        /// </summary>
        /// <param name="properties">Extraction support properties.</param>
        /// <param name="methods">Extraction support methods.</param>
        /// <param name="eventAggregator">Event service.</param>
        /// <param name="logbook">Logging service.</param>
        public ExtractionSupportEventHandling(
            IExtractionSupportProperties properties,
            IExtractionSupportMethods methods,
            IEventAggregator eventAggregator,
            ILogbook logbook) : base(logbook)
        {
            this.properties = properties;
            this.methods = methods;
            this.eventAggregator = eventAggregator;

            this.logbook.Write($"Initializing {nameof(ExtractionSupportEventHandling)}.", LogLevel.Debug);

            // Subscribe to various change events of the collection.
            properties.Bookmarks.CollectionReordered += (sender, args) => methods.UpdateEntries(properties);
            properties.Bookmarks.CollectionItemAdded += (sender, args) => methods.UpdateEntries(properties);
            properties.Bookmarks.CollectionChanged += Bookmarks_CollectionChanged;
            properties.Bookmarks.CollectionSelectedItemChanged += BookmarksSelectedItemChangedHandler;

            this.logbook.Write($"{nameof(ExtractionSupportEventHandling)} initialized.", LogLevel.Debug);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void SubscribeToEvents()
        {
            // Extraction ViewModel sends selected bookmarks and files that have been deleted from the list
            // as following events. Subscribe to said events and execute appropriate methods when needed.

            logbook.Write($"Subscribing to events.", LogLevel.Debug);

            bookmarkEventSubscription = eventAggregator
                .GetEvent<BookmarkSelectedEvent>()
                .Subscribe(x => BookmarkSelectedHandler(x, properties));
            bookmarkFileDeletedEventSubscription = eventAggregator
                .GetEvent<BookmarkFileDeletedEvent>()
                .Subscribe(x => BookmarkFileDeletedHandler(x, properties));

            logbook.Write($"Subscribed to events.", LogLevel.Debug);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            // Unsubscribe from events sent by Extraction ViewModel.

            logbook.Write($"Unsubscribing from events.", LogLevel.Debug);

            eventAggregator
                .GetEvent<BookmarkSelectedEvent>()
                .Unsubscribe(bookmarkEventSubscription);
            eventAggregator
                .GetEvent<BookmarkFileDeletedEvent>()
                .Unsubscribe(bookmarkFileDeletedEventSubscription);

            logbook.Write($"Subscription cancellation completed.", LogLevel.Debug);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ActionReset()
        {
            // When a reset has been requested by the user, clear all bookmarks from the collection.

            logbook.Write($"Resetting action state.", LogLevel.Debug);

            properties.Bookmarks.Clear();
        }

        /// <summary>
        /// Handler for bookmark selection events.
        /// </summary>
        /// <param name="info">Information basis for a new selected bookmark</param>
        /// <param name="properties">Extraction properties.</param>
        private void BookmarkSelectedHandler(BookmarkInfo info, IExtractionSupportProperties properties)
        {
            logbook.Write($"Handling bookmark selection.", LogLevel.Debug);

            // Check whether the bookmark already exists in the collection. If it does,
            // do not add it again.

            if (properties.Bookmarks.FirstOrDefault(b => b.Id == info.Id) != null)
                return;

            // Create the wrapper.
            FileAndBookmarkWrapper wrapper = CreateSelectedWrapper(info);

            // Check whether the bookmark has a parent in the collection. If it does, do
            // not add the bookmark (it is already included in the page range of the parent).

            logbook.Write($"Checking parent and children.", LogLevel.Debug);

            List<FileAndBookmarkWrapper> relatedBookmarks = properties.Bookmarks
                .Where(b => b.FilePath == wrapper.FilePath)
                .ToList();

            FileAndBookmarkWrapper? parent = wrapper.FindParent(relatedBookmarks);

            if (parent == null)
            {
                // Add new bookmark
                properties.Bookmarks.Push(wrapper);

                // If a new parent is added, remove all children of said parent from the list (they are
                // included in the range of the parent)

                IList<FileAndBookmarkWrapper> children = wrapper.FindChildren(relatedBookmarks);

                foreach (FileAndBookmarkWrapper child in children)
                {
                    properties.Bookmarks.Remove(child);
                }
            }

            logbook.Write($"Bookmark handled.", LogLevel.Debug);
        }

        private FileAndBookmarkWrapper CreateSelectedWrapper(BookmarkInfo info)
        {
            // Create inner bookmark
            LeveledBookmark innerMark = new LeveledBookmark(
                1, info.Title, info.StartPage, info.EndPage - info.StartPage + 1);

            // Create the wrapper.
            return new FileAndBookmarkWrapper(innerMark, info.FilePath, 0, info.Id);
        }

        /// <summary>
        /// Handler for file deletion events.
        /// </summary>
        /// <param name="path">Filepath of the file that was removed by the user</param>
        /// <param name="properties">Extraction support properties.</param>
        private void BookmarkFileDeletedHandler(string path, IExtractionSupportProperties properties)
        {
            // Remove all bookmarks with a corresponding path from the collection (since
            // the relevant document was also removed).

            logbook.Write($"Handling file removal.", LogLevel.Debug);

            properties.Bookmarks.RemoveAll(b => b.FilePath == path);

            logbook.Write($"File removal handled.", LogLevel.Debug);
        }

        /// <summary>
        /// Handle changes in the selected item of the collection. Notifies relevant
        /// properties of the change, updating UI.
        /// </summary>
        /// <param name="sender">Sending collection</param>
        /// <param name="e"><see cref="CollectionSelectedItemChangedEventArgs{T}"/></param>
        private void BookmarksSelectedItemChangedHandler(
            object? sender,
            CollectionSelectedItemChangedEventArgs<FileAndBookmarkWrapper>? e)
        {
            properties.RaiseChanged(nameof(properties.IsSelectedActualBookmark));
        }

        /// <summary>
        /// Handle all changes in the collection. Updates entries (their 
        /// <see cref="FileAndBookmarkWrapper.Index"/> property) and produces relevant notifications.
        /// </summary>
        /// <param name="sender">Sending collection</param>
        /// <param name="e">Event arguments</param>
        private void Bookmarks_CollectionChanged(
            object? sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            methods.UpdateEntries(properties);
        }
    }
}
