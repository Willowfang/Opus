using System;
using System.Collections.Generic;
using System.Text;

namespace PDFLib.PDF.Services
{
    public interface ISignature
    {
        void Remove(IFilePDF file, string destDirectory, string identifier);
        void Remove(IFilePDF[] sourceFiles, string destDirectory, string identifier);
        void RemoveCMD(string filePath, string identifier);
    }
}
