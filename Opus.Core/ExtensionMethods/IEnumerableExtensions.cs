using CX.PdfLib.Services.Data;
using Opus.Core.Wrappers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opus.Core.ExtensionMethods
{
    public static class IEnumerableExtensions
    {
        public static IList<ILeveledBookmark> FromStorage(this IEnumerable<BookmarkStorage> storage)
        {
            List<ILeveledBookmark> original = new List<ILeveledBookmark>();
            foreach (BookmarkStorage bms in storage)
            {
                original.Add(bms.Value);
            }
            return original;
        }

        public static ObservableCollection<BookmarkStorage> ToStorage(this IEnumerable<ILeveledBookmark> collection)
        {
            ObservableCollection<BookmarkStorage> converted = new ObservableCollection<BookmarkStorage>();
            foreach (ILeveledBookmark bookmark in collection)
            {
                converted.Add(new BookmarkStorage(bookmark));
            }
            return converted;
        }
    }
}
