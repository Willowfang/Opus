using Opus.Services.Implementation.Data.Extraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Extensions
{
    public static class FileAndBookmarkWrapperExtensions
    {
        public static List<FileAndBookmarkWrapper> Extractables(
            this IEnumerable<FileAndBookmarkWrapper> original
        )
        {
            return original.Where(x => x.Bookmark.Pages.Count >= 1).ToList();
        }
    }
}
