using CX.PdfLib.Common;
using CX.PdfLib.Services.Data;
using Opus.Services.Configuration;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CX.PdfLib.Extensions;
using Opus.Services.Implementation.Data.Extraction;
using System.IO;
using CX.LoggingLib;
using Opus.ExtensionMethods;

namespace Opus.Services.Implementation.StaticHelpers
{
    public static class BookmarkMethods
    {
        public static async Task<string> AskForTitle(IDialogAssist dialogAssist,
            IConfiguration configuration)
        {
            string title = configuration.ExtractionTitle;

            if (configuration.ExtractionTitleAsk)
            {
                ExtractSettingsDialog dialog = new ExtractSettingsDialog(
                    Resources.Labels.Dialogs.ExtractionOptions.AskDialogTitle, true)
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

        public static IEnumerable<ILeveledBookmark> GetParentsOnly(IEnumerable<ILeveledBookmark> originals)
        {
            List<ILeveledBookmark> parents = new List<ILeveledBookmark>();
            foreach (ILeveledBookmark original in originals)
            {
                if (original.IsChild(originals) == false)
                    parents.Add(original);
            }

            return parents;
        }

        public static IEnumerable<FileAndBookmarkWrapper> GetRenamedAndIndexed(IEnumerable<ILeveledBookmark> bookmarks, IList<FileAndBookmarkWrapper> order, string titleTemplate, string filePath, ILogbook? logbook = null)
        {
            int numberCount = bookmarks.Count().ToString().Length;

            IList<FileAndBookmarkWrapper> added = new List<FileAndBookmarkWrapper>();
            foreach (ILeveledBookmark bookmark in bookmarks)
            {
                FileAndBookmarkWrapper? compare = order.FirstOrDefault(w => w.FilePath == filePath 
                && w.Bookmark == bookmark);
                int index = compare != null ? order.IndexOf(compare) : 0;

                string bmReplace = Resources.Placeholders.FileNames.Bookmark;
                string fileReplace = Resources.Placeholders.FileNames.File;
                string numberReplace = Resources.Placeholders.FileNames.Number;

                string title = titleTemplate;
                title = title.ReplacePlaceholder(Placeholders.Bookmark, bookmark.Title);

                if (title.Contains(bmReplace))
                {
                    title = title.Replace(bmReplace, bookmark.Title);
                }
                if (title.Contains(fileReplace))
                {
                    title = title.Replace(fileReplace, Path.GetFileNameWithoutExtension(filePath));
                }
                if (title.Contains(numberReplace))
                {
                    string countString = (index + 1).ToString();
                    string numberReplacementString = countString;
                    int zeroCount = numberCount - countString.Length;
                    if (zeroCount > 0)
                    {
                        numberReplacementString = string.Concat(Enumerable.Repeat("0", zeroCount)) +
                            numberReplacementString;
                    }

                    title = title.Replace(numberReplace, numberReplacementString);
                }

                int identicalCount = added.Where(b => b.Bookmark.Title == title).Count();

                if (identicalCount > 0)
                {
                    title = $"{title} {identicalCount + 1}";
                }

                ILeveledBookmark renamed = new LeveledBookmark(bookmark.Level, title, bookmark.Pages);
                added.Add(new FileAndBookmarkWrapper(renamed, filePath, index));
            }

            if (logbook != null)
                logbook.Write($"{nameof(FileAndBookmarkWrapper)}s renamed according to {nameof(titleTemplate)} '{titleTemplate}' and indexed by {nameof(IEnumerable<ILeveledBookmark>)} {bookmarks}.", LogLevel.Debug);

            return added;
        }

        public static IList<ILeveledBookmark> AdjustLevels(IEnumerable<ILeveledBookmark> bookmarks,
            int adjustment)
        {
            IList<ILeveledBookmark> adjusted = new List<ILeveledBookmark>();
            foreach (var bookmark in bookmarks)
            {
                adjusted.Add(new LeveledBookmark(bookmark.Level + adjustment, bookmark.Title, bookmark.Pages));
            }

            return adjusted;
        }
    }
}
