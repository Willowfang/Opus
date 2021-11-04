using PDFLib.Implementation;
using System.Collections.Generic;

namespace PDFLib.Services
{
    public interface IBookmarkOperator
    {
        List<Bookmark> GetBookmarks(string sourceFile);
        List<IBookmark> ImportBookmarks(string sourceFile, int documentEndPage);
        int GetLastPage(string sourceFile);
    }
}
