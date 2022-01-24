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

namespace Opus.Services.Implementation.StaticHelpers
{
    public static class BookmarkMethods
    {
        public static async Task<(string prefix, string suffix)> AskForAffixes(IDialogAssist dialogAssist, 
            IConfiguration configuration)
        {
            string prefix = configuration.ExtractionPrefix;
            string suffix = configuration.ExtractionSuffix;

            if (configuration.ExtractionPrefixSuffixAsk)
            {
                ExtractSettingsDialog dialog = new ExtractSettingsDialog(Resources.Labels.General.Settings, true)
                {
                    Prefix = configuration.ExtractionPrefix,
                    Suffix = configuration.ExtractionSuffix
                };

                await dialogAssist.Show(dialog);

                prefix = dialog.Prefix;
                suffix = dialog.Suffix;
            }

            return (prefix, suffix);
        }

        public static IEnumerable<ILeveledBookmark> AddAffixes(IEnumerable<ILeveledBookmark> bookmarks,
            string prefix, string suffix)
        {
            IList<ILeveledBookmark> added = new List<ILeveledBookmark>();
            foreach (ILeveledBookmark bookmark in bookmarks)
            {
                string title = string.Empty;
                if (!string.IsNullOrEmpty(prefix))
                {
                    title = prefix + " ";
                }
                title = title + bookmark.Title;
                if (!string.IsNullOrEmpty(suffix))
                {
                    title = title + " " + suffix;
                }

                added.Add(new LeveledBookmark(bookmark.Level, title, bookmark.Pages));
            }

            return added;
        }
    }
}
