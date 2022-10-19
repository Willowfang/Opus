using CX.PdfLib.Common;
using CX.PdfLib.Services.Data;
using Opus.Services.Configuration;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CX.PdfLib.Extensions;
using Opus.Services.Implementation.Data.Extraction;
using System.IO;
using CX.LoggingLib;
using Opus.ExtensionMethods;

namespace Opus.Services.Implementation.StaticHelpers
{
    /// <summary>
    /// Helper methods for bookmark manipulations.
    /// </summary>
    public static class BookmarkMethods
    {
        /// <summary>
        /// Ask the user for a name template.
        /// </summary>
        /// <param name="dialogAssist">Service for showing dialogs.</param>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <returns>An awaitable task. The task returns the given name template.</returns>
        public static async Task<string> AskForTitle(
            IDialogAssist dialogAssist,
            IConfiguration configuration
        )
        {
            string title = configuration.ExtractionTitle;

            if (configuration.ExtractionTitleAsk)
            {
                ExtractSettingsDialog dialog = new ExtractSettingsDialog(
                    Resources.Labels.Dialogs.ExtractionOptions.AskDialogTitle,
                    true
                )
                {
                    Title = configuration.ExtractionTitle
                };

                await dialogAssist.Show(dialog);

                title = dialog.Title;
            }

            if (string.IsNullOrEmpty(title))
                title = Resources.Placeholders.FileNames.Bookmark;

            return title;
        }

        /// <summary>
        /// Return only bookmarks that are parents of another bookmark.
        /// </summary>
        /// <param name="originals">Bookmarks to search the parents from.</param>
        /// <returns>Found parents, if any.</returns>
        public static IEnumerable<ILeveledBookmark> GetParentsOnly(
            IEnumerable<ILeveledBookmark> originals
        )
        {
            List<ILeveledBookmark> parents = new List<ILeveledBookmark>();
            foreach (ILeveledBookmark original in originals)
            {
                if (original.IsChild(originals) == false)
                    parents.Add(original);
            }

            return parents;
        }

        /// <summary>
        /// Return bookmarks that have been renamed according to the template.
        /// </summary>
        /// <param name="order">Bookmark wrappers to modify.</param>
        /// <param name="titleTemplate">Template to apply.</param>
        /// <param name="logbook">Logging service.</param>
        /// <returns>Renamed wrappers.</returns>
        public static IEnumerable<FileAndBookmarkWrapper> GetRenamed(
            IList<FileAndBookmarkWrapper> order,
            string titleTemplate,
            ILogbook? logbook = null
        )
        {
            // The constant amount of numbers added in the name of every file (e.g. if more than 10 files: 01,
            // 02, 03 etc).
            int numberCount = order.Last().Index.ToString().Length;

            // The list to return
            IList<FileAndBookmarkWrapper> added = new List<FileAndBookmarkWrapper>();

            foreach (FileAndBookmarkWrapper bookmark in order)
            {
                if (bookmark.Bookmark.Pages.Count == 0)
                    continue;

                string bmReplace = Resources.Placeholders.FileNames.Bookmark;
                string fileReplace = Resources.Placeholders.FileNames.File;
                string numberReplace = Resources.Placeholders.FileNames.Number;

                string title = titleTemplate;
                title = title.ReplacePlaceholder(Placeholders.Bookmark, bookmark.Bookmark.Title);

                if (title.Contains(bmReplace))
                {
                    title = title.Replace(bmReplace, bookmark.Bookmark.Title);
                }
                if (title.Contains(fileReplace))
                {
                    title = title.Replace(
                        fileReplace,
                        Path.GetFileNameWithoutExtension(bookmark.FilePath)
                    );
                }
                if (title.Contains(numberReplace))
                {
                    string countString = bookmark.Index.ToString();
                    string numberReplacementString = countString;
                    int zeroCount = numberCount - countString.Length;
                    if (zeroCount > 0)
                    {
                        numberReplacementString =
                            string.Concat(Enumerable.Repeat("0", zeroCount))
                            + numberReplacementString;
                    }

                    title = title.Replace(numberReplace, numberReplacementString);
                }

                int identicalCount = added.Where(b => b.Bookmark.Title == title).Count();

                if (identicalCount > 0)
                {
                    title = $"{title} {identicalCount + 1}";
                }

                ILeveledBookmark renamed = new LeveledBookmark(
                    bookmark.Level,
                    title,
                    bookmark.Bookmark.Pages
                );
                added.Add(
                    new FileAndBookmarkWrapper(
                        renamed,
                        bookmark.FilePath,
                        bookmark.Index,
                        bookmark.Id
                    )
                );
            }

            if (logbook != null)
                logbook.Write(
                    $"{nameof(FileAndBookmarkWrapper)}s renamed according to {nameof(titleTemplate)} '{titleTemplate}'.",
                    LogLevel.Debug
                );

            return added;
        }

        /// <summary>
        /// Adjust all levels of bookmarks in a list.
        /// </summary>
        /// <param name="bookmarks">Bookmarks to adjust levels for.</param>
        /// <param name="adjustment">Amount to adjust.</param>
        /// <returns></returns>
        public static IList<ILeveledBookmark> AdjustLevels(
            IEnumerable<ILeveledBookmark> bookmarks,
            int adjustment
        )
        {
            IList<ILeveledBookmark> adjusted = new List<ILeveledBookmark>();
            foreach (var bookmark in bookmarks)
            {
                adjusted.Add(
                    new LeveledBookmark(bookmark.Level + adjustment, bookmark.Title, bookmark.Pages)
                );
            }

            return adjusted;
        }

        /// <summary>
        /// Return an empty wrapper.
        /// </summary>
        /// <param name="title">Name of the wrapper.</param>
        /// <param name="fileName">String to use as filename.</param>
        /// <param name="index">Index in the sequence of bookmarks.</param>
        /// <returns>Empty wrapper.</returns>
        public static FileAndBookmarkWrapper GetPlaceHolderBookmarkWrapper(
            string title,
            string fileName,
            int index = 0
        )
        {
            return new FileAndBookmarkWrapper(new LeveledBookmark(0, title, 0, 0), fileName, index);
        }
    }
}
