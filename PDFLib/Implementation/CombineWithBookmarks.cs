using PDFLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFLib.Implementation
{
    public class CombineWithBookmarks : IJoin
    {
        private IBookmarkOperator BookmarkOperator;

        public CombineWithBookmarks(IBookmarkOperator bmOperator) => BookmarkOperator = bmOperator;

        public void Combine(string[] filePaths, string outputDirectory)
        {
            
        }
    }
}
