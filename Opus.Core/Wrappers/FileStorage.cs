using CX.PdfLib.Services.Data;
using Prism.Mvvm;
using System.IO;

namespace Opus.Core.Wrappers
{
    public class FileStorage : BindableBase, ILeveledItem
    {
        private bool isSelected;
        private int level;
        private string title;

        public string FilePath { get; }
        public string FileName { get; }
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
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
            FileName = Path.GetFileName(filePath);
            Level = 1;
            Title = Path.GetFileNameWithoutExtension(filePath);
        }
    }
}
