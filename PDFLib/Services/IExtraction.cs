using System;
using System.Collections.Generic;
using System.Text;

namespace PDFLib.Services
{
    public interface IExtraction
    {
        void ExtractSeparate(string sourceFile, string destDirectory, IEnumerable<IBookmark> allBookMarks);
        void Extract(string sourceFile, string destFile, IEnumerable<IBookmark> allBookMarks);
        void ExtractCMD(string sourceFile, string destDirectory, string selectPrefix = null);
    }
}
