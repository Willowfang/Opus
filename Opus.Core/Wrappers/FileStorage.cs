using WF.PdfLib.Services.Data;
using Opus.Services.Base;
using Prism.Mvvm;
using System.IO;

namespace Opus.Core.Wrappers
{
    /// <summary>
    /// Class for storing information about a file.
    /// </summary>
    public class FileStorage : BindableBase, ILeveledItem, ISelectable
    {
        // Backing properties

        private bool isSelected;
        private int level;
        private string title;

        /// <summary>
        /// Path to the file contained in this container.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Name of the contained file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Title of the container shown to the user.
        /// </summary>
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        /// <summary>
        /// Level of the container in the hierarchy (indentation).
        /// </summary>
        public int Level
        {
            get => level;
            set => SetProperty(ref level, value);
        }

        /// <summary>
        /// Has the file been selected by the user or otherwise?
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        /// <summary>
        /// Create a new container for storing information about a file.
        /// </summary>
        /// <param name="filePath">Path to the file to include in the container..</param>
        public FileStorage(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Level = 1;
            Title = Path.GetFileNameWithoutExtension(filePath);
        }
    }
}
