using System;
using System.Collections.Generic;
using System.Text;

namespace PDFLib.PDF.Services
{
    public interface IBookmark
    {
        Guid Id { get; }
        Guid ParentId { get; set; }

        string Title { get; }
        int StartPage { get; set; }
        int EndPage { get; set; }
        int Level { get; set; }
        string Range { get; }
        bool IsSelected { get; set; }
    }
}
