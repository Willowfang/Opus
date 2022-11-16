using Opus.Actions.Services.Base;
using Opus.Common.Wrappers;
using System.Collections.ObjectModel;

namespace Opus.Actions.Services.WorkCopy
{
    /// <summary>
    /// Properties for work copy action.
    /// </summary>
    public interface IWorkCopyProperties : IActionProperties
    {
        /// <summary>
        /// Collection for files that will have their signatures removed.
        /// </summary>
        public ObservableCollection<FileStorage> OriginalFiles { get; set; }

        /// <summary>
        /// The file that is currently selected.
        /// </summary>
        public FileStorage? SelectedFile { get; set; }
    }
}
