using Opus.Common.Wrappers;
using Opus.Common.Helpers;
using Opus.Common.Services.Dialogs;
using Prism.Commands;
using Opus.Common.Collections;

namespace Opus.Common.Dialogs
{
    /// <summary>
    /// A dialog for rearranging the bookmarks to extract (for correct numbering).
    /// <para>
    /// This dialog is only used in context menu actions. In GUI the rearranging happens within the same
    /// view as the selection.
    /// </para>
    /// </summary>
    public class ExtractOrderDialog : DialogBase, IDialog
    {
        /// <summary>
        /// Bookmarks to arrange.
        /// </summary>
        public ReorderCollection<FileAndBookmarkWrapper> Bookmarks { get; }

        /// <summary>
        /// If true, produce a single file.
        /// </summary>
        public bool SingleFile { get; }

        private bool groupByFiles;

        /// <summary>
        /// If producing a single file and this is true, bookmarks will be grouped within that file according
        /// to their original source files.
        /// </summary>
        public bool GroupByFiles
        {
            get => groupByFiles;
            set => SetProperty(ref groupByFiles, value);
        }

        private DelegateCommand? addExternal;

        /// <summary>
        /// Command for adding a marker for an external file (for numbering purposes).
        /// </summary>
        public DelegateCommand AddExternal =>
            addExternal ??= new DelegateCommand(ExecuteAddExternal);

        /// <summary>
        /// Create a new dialog for selecting extraction order.
        /// </summary>
        /// <param name="title">Title for the dialog.</param>
        /// <param name="singleFile">Produce a single file.</param>
        /// <param name="groupByFiles">If true, group bookmarks by their source within the file.</param>
        public ExtractOrderDialog(string title, bool singleFile, bool groupByFiles) : base(title)
        {
            Bookmarks = new ReorderCollection<FileAndBookmarkWrapper>();
            Bookmarks.CanReorder = true;
            SingleFile = singleFile;
            GroupByFiles = groupByFiles;
            Bookmarks.CollectionReordered += (sender, args) => UpdateIndexes();
            Bookmarks.CollectionItemAdded += (sender, args) => UpdateIndexes();
            Bookmarks.CollectionChanged += Bookmarks_CollectionChanged;
        }

        private void Bookmarks_CollectionChanged(
            object? sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
        {
            UpdateIndexes();
        }

        private void ExecuteAddExternal()
        {
            Bookmarks.Add(
                BookmarkMethods.GetPlaceHolderBookmarkWrapper(
                    Resources.Labels.Dialogs.ExtractionOrder.ExternalFile,
                    Resources.Labels.Dialogs.ExtractionOrder.ExternalFile,
                    Bookmarks.Count + 1
                )
            );
        }

        /// <summary>
        /// Update all the indexes of all bookmark wrappers (to correspond to their index in the collection).
        /// </summary>
        public void UpdateIndexes()
        {
            for (int i = 0; i < Bookmarks.Count; i++)
            {
                Bookmarks[i].Index = i + 1;
            }
        }
    }
}
