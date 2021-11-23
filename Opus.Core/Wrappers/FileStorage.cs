using Prism.Mvvm;
using System.IO;

namespace Opus.Core.Wrappers
{
    public class FileStorage : BindableBase
    {
        private bool isSelected;

        public string FilePath { get; }
        public string Title => Path.GetFileNameWithoutExtension(FilePath);
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public FileStorage(string filePath)
        {
            FilePath = filePath;
        }
    }
}
