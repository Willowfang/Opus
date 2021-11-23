using CX.PdfLib.Services.Data;
using Opus.Core.Wrappers;
using System.Collections.Generic;

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
    }
}
