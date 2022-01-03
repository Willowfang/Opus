using CX.PdfLib.Services.Data;
using Opus.Core.Wrappers;
using System.Collections.Generic;
using System.Linq;

namespace Opus.Core.ExtensionMethods
{
    public static class IListExtensions
    {
        public static IList<BookmarkStorage> ToStorage(this IList<ILeveledBookmark> original)
        {
            List<BookmarkStorage> converted = new List<BookmarkStorage>();
            foreach (ILeveledBookmark bm in original)
            {
                converted.Add(new BookmarkStorage(bm));
            }
            return converted;
        }

        public static BookmarkStorage FindParent(this IList<BookmarkStorage> storage, int childStartPage,
            int childEndPage)
        {
            return storage.LastOrDefault(x =>
                (x.Value.StartPage < childStartPage && x.Value.EndPage == childEndPage) ||
                (x.Value.StartPage <= childStartPage && x.Value.EndPage > childEndPage));
        }
        public static BookmarkStorage FindPrecedingSibling(this IList<BookmarkStorage> storage, 
            BookmarkStorage current, BookmarkStorage commonParent)
        {

            if (commonParent == null)
                return storage.LastOrDefault(x =>
                    x.Value.Level == 1 &&
                    x.Value.StartPage <= current.Value.StartPage &&
                    x.Value.EndPage < current.Value.EndPage);

            return storage.LastOrDefault(x =>
                x.Value.StartPage >= commonParent.Value.StartPage &&
                x.Value.EndPage <= commonParent.Value.EndPage &&
                x.Value.StartPage < current.Value.StartPage);
        }
        public static IList<BookmarkStorage> FindChildren(this IList<BookmarkStorage> storage,
            BookmarkStorage parent)
        {
            return storage.Where(x =>
                (x.Value.StartPage > parent.Value.StartPage && x.Value.EndPage == parent.Value.EndPage) ||
                (x.Value.StartPage >= parent.Value.StartPage && x.Value.EndPage < parent.Value.EndPage)).ToList();
        }
    }
}
