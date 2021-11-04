using System;
using System.Collections.Generic;
using System.Text;

namespace PDFLib.PDF.Services
{
    public interface IFilePDF
    {
        string Title { get; }
        string FilePath { get; }
        bool IsSelected { get; set; }
    }
}
