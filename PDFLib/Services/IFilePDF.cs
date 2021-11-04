using System;
using System.Collections.Generic;
using System.Text;

namespace PDFLib.Services
{
    public interface IFilePDF
    {
        string Title { get; }
        string FilePath { get; }
        bool IsSelected { get; set; }
    }
}
