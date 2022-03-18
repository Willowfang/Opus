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
                ExtractSettingsDialog dialog = new ExtractSettingsDialog(Resources.Labels.General.Settings, true)
                {
                    Title = configuration.ExtractionTitle
                };

                await dialogAssist.Show(dialog);

                title = dialog.Title;
            }

            if (string.IsNullOrEmpty(title))
                title = Resources.DefaultValues.DefaultValues.Bookmark;

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

        public static IEnumerable<FileAndBookmarkWrapper> GetRenamedAndIndexed(IEnumerable<ILeveledBookmark> bookmarks, IList<FileAndBookmarkWrapper> order, string titleTemplate, string filePath)
        {
            IList<FileAndBookmarkWrapper> added = new List<FileAndBookmarkWrapper>();
            foreach (ILeveledBookmark bookmark in bookmarks)
            {
                FileAndBookmarkWrapper? compare = order.FirstOrDefault(w => w.FilePath == filePath 
                && w.Bookmark == bookmark);
                int index = compare != null ? order.IndexOf(compare) : 0;

                string bmReplace = Resources.DefaultValues.DefaultValues.Bookmark;
                string fileReplace = Resources.DefaultValues.DefaultValues.File;
                string numberReplace = Resources.DefaultValues.DefaultValues.Number;

                string title = titleTemplate;
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
                    title = title.Replace(numberReplace, (index + 1).ToString());
                }

                int identicalCount = added.Where(b => b.Bookmark.Title == title).Count();

                if (identicalCount > 0)
                {
                    title = $"{title} {identicalCount + 1}";
                }

                ILeveledBookmark renamed = new LeveledBookmark(bookmark.Level, title, bookmark.Pages);
                added.Add(new FileAndBookmarkWrapper(renamed, filePath, index));
            }

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
