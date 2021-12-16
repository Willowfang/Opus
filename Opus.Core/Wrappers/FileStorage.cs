using CX.PdfLib.Services.Data;
using Prism.Mvvm;
using System.IO;

namespace Opus.Core.Wrappers
{
    public class FileStorage : BindableBase, ILeveledItem
    {
        private bool isSelected;
        private int level;

        public string FilePath { get; }
        public string Title => Path.GetFileNameWithoutExtension(FilePath);
        public int Level
        {
            get => level;
            set => SetProperty(ref level, value);
        }
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public FileStorage(string filePath)
        {
            FilePath = filePath;
            Level = 1;
        }
    }
}
