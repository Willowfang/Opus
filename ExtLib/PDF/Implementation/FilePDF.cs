using PDFLib.PDF.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PDFLib.PDF.Implementation
{
    public class FilePDF : BaseClasses.ModelBase, IFilePDF
    {
        public string Title
        {
            get => Path.GetFileNameWithoutExtension(FilePath);
        }
        public string FilePath { get; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public FilePDF(string filePath)
        {
            FilePath = filePath;
        }
    }
}
