using Opus.Common.Services.Dialogs;

namespace Opus.Common.Dialogs
{
    /// <summary>
    /// A dialog for choosing setting for composition.
    /// </summary>
    public class CompositionSettingsDialog : DialogBase, IDialog
    {
        private bool searchSubDirectories;

        /// <summary>
        /// If true, search for subdirectories when searching for files.
        /// </summary>
        public bool SearchSubDirectories
        {
            get => searchSubDirectories;
            set => SetProperty(ref searchSubDirectories, value);
        }

        private bool deleteConverted;

        /// <summary>
        /// If true, delete converted files when composition has finished.
        /// </summary>
        public bool DeleteConverted
        {
            get => deleteConverted;
            set => SetProperty(ref deleteConverted, value);
        }

        /// <summary>
        /// Create a new dialog for choosing the correct settings for composition.
        /// </summary>
        /// <param name="dialogTitle"></param>
        public CompositionSettingsDialog(string dialogTitle) : base(dialogTitle) { }
    }
}
