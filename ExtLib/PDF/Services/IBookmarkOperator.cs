using PDFLib.PDF.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDFLib.PDF.Services
{
    public interface IBookmarkOperator
    {
        List<Bookmark> GetBookmarks(string sourceFile);
        List<IBookmark> ImportBookmarks(string sourceFile, int documentEndPage);
        int GetLastPage(string sourceFile);
    }
}
