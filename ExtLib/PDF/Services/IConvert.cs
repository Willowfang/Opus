using System;
using System.Collections.Generic;
using System.Text;

namespace PDFLib.PDF.Services
{
    public interface IConvert
    {
        IFilePDF Convert(string filePath);
    }
}
