using System;
using System.Collections.Generic;
using System.Text;

namespace PDFLib.Services
{
    public interface IConvert
    {
        string Convert(string filePath, string outputDirectory);
        string[] Convert(string[] filePaths, string outputDirectory);
    }
}
