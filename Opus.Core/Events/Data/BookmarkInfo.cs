using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.Events.Data
{
    public class BookmarkInfo
    {
        public int StartPage { get; }
        public int EndPage { get; }
        public string Title { get; }

        public BookmarkInfo(int startPage, int endPage, string title)
        {
            StartPage = startPage;
            EndPage = endPage;
            Title = title;
        }
    }
}
