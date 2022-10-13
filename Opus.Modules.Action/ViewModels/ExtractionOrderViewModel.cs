using AsyncAwaitBestPractices.MVVM;
using CX.LoggingLib;
using Opus.Events.Data;
using Opus.Services.Implementation.Data.Extraction;
using Opus.Services.Implementation.StaticHelpers;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using Prism.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Events;
using Opus.Core.Executors;
using Opus.Core.Base;
using CX.PdfLib.Common;
using Opus.Events;
using Opus.Values;
using Opus.Services.Extensions;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// ViewModel for organizing bookmarks for extraction. Depends on more general extraction services.
    /// </summary>
    public class ExtractionOrderViewModel : ViewModelBaseLogging<ExtractionViewModel>, INavigationTarget
    {
        #region DI services
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogAssist dialogAssist;
        private readonly IExtractionExecutor executor;
        #endregion

        #region Properties
        /// <summary>
        /// Collection for the bookmarks to extract.
        /// </summary>
        /// <remarks>See <see cref="ReorderCollection{T}"/> for more information on properties and methods on organizing the bookmarks before extraction.</remarks>
        public ReorderCollection<FileAndBookmarkWrapper> Bookmarks { get; }

        /// <summary>
        /// Check whether currently selected item is an actual bookmark from a document (rather than a placeholder or null).
        /// Changes are notified when selected item changes.
        /// </summary>
        public bool IsSelectedActualBookmark
        {
            get => Bookmarks.SelectedItem != null && Bookmarks.SelectedItem.Bookmark.Pages.Count > 0;
        }

        /// <summary>
        /// Check whether the collection contains´any items and whether any of those items are actual bookmarks
        /// from a document (rather than just placeholders). Changes are notified on collection changes.
        /// </summary>
        public bool CollectionHasActualBookmarks
        {
            get
            {
                bool value = false;
                foreach (FileAndBookmarkWrapper wrapper in Bookmarks)
                {
                    if (wrapper.Bookmark.Pages.Count > 0)
                    {
                        value = true;
                        break;
                    }
                }
                return value;
            }
        }
        #endregion

        /// <summary>
        /// ViewModel for extraction bookmark ordering.
        /// </summary>
        /// 
        /// <param name="eventAggregator"><see cref="IEventAggregator"/></param>
        /// <param name="dialogAssist"><see cref="IDialogAssist"/></param>
        /// <param name="navregistry"></param>
        /// <param name="executor"></param>
        /// <param name="logbook"></param>
        public ExtractionOrderViewModel(
            IEventAggregator eventAggregator,
            IDialogAssist dialogAssist,
            INavigationTargetRegistry navregistry,
            IExtractionExecutor executor,
            ILogbook logbook) : base(logbook)
        {
            this.eventAggregator = eventAggregator;
            this.dialogAssist = dialogAssist;
            this.executor = executor;
            navregistry.AddTarget(SchemeNames.SPLIT, this);
            Bookmarks = new ReorderCollection<FileAndBookmarkWrapper>();
            Bookmarks.CanReorder = true;
            Bookmarks.CollectionReordered += (sender, args) => UpdateEntries();
            Bookmarks.CollectionItemAdded += (sender, args) => UpdateEntries();
            Bookmarks.CollectionChanged += Bookmarks_CollectionChanged;
            Bookmarks.CollectionSelectedItemChanged += Bookmarks_CollectionSelectedItemChanged;
        }

        SubscriptionToken bookmarkEventSubscription;
        SubscriptionToken bookmarkFileDeletedEventSubscription;
        public void OnArrival()
        {
            bookmarkEventSubscription = eventAggregator.GetEvent<BookmarkSelectedEvent>().Subscribe(BookmarkSelected);
            bookmarkFileDeletedEventSubscription = eventAggregator.GetEvent<BookmarkFileDeletedEvent>().Subscribe(BookmarkFileDeleted);

            logbook.Write($"{this} subscribed to {nameof(BookmarkSelectedEvent)}.", LogLevel.Debug);
        }
        public void WhenLeaving()
        {
            eventAggregator.GetEvent<BookmarkSelectedEvent>().Unsubscribe(bookmarkEventSubscription);
            eventAggregator.GetEvent<BookmarkFileDeletedEvent>().Unsubscribe(bookmarkFileDeletedEventSubscription);

            logbook.Write($"{this} unsubscribed from {nameof(BookmarkSelectedEvent)}.", LogLevel.Debug);
        }

        public void Reset()
        {
            Bookmarks.Clear();
        }

        private void BookmarkSelected(BookmarkInfo info)
        {
            // Filter out bookmarks that already exist in the collection
            if (Bookmarks.FirstOrDefault(b => b.Id == info.Id) != null) return;

            FileAndBookmarkWrapper wrapper = new FileAndBookmarkWrapper(
                new LeveledBookmark(
                    1,
                    info.Title,
                    info.StartPage,
                    info.EndPage - info.StartPage + 1),
                info.FilePath,
                0,
                info.Id);

            // Filter out bookmarks that already have a parent in the collection
            List<FileAndBookmarkWrapper> relatedBookmarks = Bookmarks.Where(b => b.FilePath == wrapper.FilePath).ToList();
            FileAndBookmarkWrapper parent = wrapper.FindParent(relatedBookmarks);

            if (parent == null)
            {
                Bookmarks.Push(wrapper);

                // If a new parent is added, remove all children of said parent from the list (they are
                // included in the range of the parent)
                IList<FileAndBookmarkWrapper> children = wrapper.FindChildren(relatedBookmarks);
                foreach (FileAndBookmarkWrapper child in children)
                {
                    Bookmarks.Remove(child);
                }
            }
        }
        private void BookmarkFileDeleted(string path)
        {
            Bookmarks.RemoveAll(b => b.FilePath == path);
        }

        private void Bookmarks_CollectionSelectedItemChanged(object sender, CollectionSelectedItemChangedEventArgs<FileAndBookmarkWrapper> e)
        {
            RaisePropertyChanged(nameof(IsSelectedActualBookmark));
        }

        private void Bookmarks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateEntries();
        }

        private DelegateCommand addExternal;
        public DelegateCommand AddExternal => addExternal ??= new DelegateCommand(ExecuteAddExternal);
        private void ExecuteAddExternal()
        {
            Bookmarks.Add(BookmarkMethods.GetPlaceHolderBookmarkWrapper(Resources.Labels.Dialogs.ExtractionOrder.ExternalFile, Resources.Labels.Dialogs.ExtractionOrder.ExternalFile, Bookmarks.Count + 1));
        }

        private IAsyncCommand editCommand;
        public IAsyncCommand EditCommand =>
            editCommand ??= new AsyncCommand(ExecuteEditCommand);

        private async Task ExecuteEditCommand()
        {
            if (Bookmarks.SelectedItem == null)
            {
                return;
            }

            BookmarkDialog dialog = new BookmarkDialog(Resources.Labels.Dialogs.Bookmark.Edit)
            {
                Title = Bookmarks.SelectedItem.Bookmark.Title,
                StartPage = Bookmarks.SelectedItem.Bookmark.StartPage,
                EndPage = Bookmarks.SelectedItem.Bookmark.EndPage
            };

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                logbook.Write($"Cancellation requested at {nameof(IDialog)} '{dialog}'.", LogLevel.Information);
                return;
            }

            FileAndBookmarkWrapper updated = new FileAndBookmarkWrapper(
                new LeveledBookmark(Bookmarks.SelectedItem.Bookmark.Level, dialog.Title, dialog.StartPage,
                dialog.EndPage - dialog.StartPage + 1), Bookmarks.SelectedItem.FilePath, Bookmarks.SelectedItem.Index,
                Bookmarks.SelectedItem.Id);

            int index = Bookmarks.IndexOf(Bookmarks.SelectedItem);
            Bookmarks.RemoveSelected();
            Bookmarks.Insert(index, updated);

            logbook.Write($"{nameof(BookmarkInfo)} '{updated.Bookmark.Title}' edited.", LogLevel.Information);
        }

        private DelegateCommand deleteCommand;
        public DelegateCommand DeleteCommand =>
            deleteCommand ?? (deleteCommand = new DelegateCommand(ExecuteDeleteCommand));

        void ExecuteDeleteCommand()
        {
            eventAggregator.GetEvent<BookmarkDeselectedEvent>().Publish(Bookmarks.SelectedItem.Id);
            Bookmarks.RemoveSelected();
            if (Bookmarks.All(b => b.Bookmark.Pages.Count == 0))
            {
                Bookmarks.Clear();
            }
        }

        public void UpdateEntries()
        {
            for (int i = 0; i < Bookmarks.Count; i++)
            {
                Bookmarks[i].Index = i + 1;
            }

            RaisePropertyChanged(nameof(CollectionHasActualBookmarks));
        }
    }
}
