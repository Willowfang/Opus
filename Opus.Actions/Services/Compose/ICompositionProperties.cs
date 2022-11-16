using Opus.Actions.Services.Base;
using Opus.Common.Services.Data.Composition;
using System.Collections.ObjectModel;

namespace Opus.Actions.Services.Compose
{
    /// <summary>
    /// Properties for composition action.
    /// </summary>
    public interface ICompositionProperties : IActionProperties
    {
        /// <summary>
        /// All profiles stored in the database. Composition profiles
        /// determine what files are to be added and in what order, as well as
        /// their corresponding info for adding bookmarks.
        /// </summary>
        public ObservableCollection<ICompositionProfile> Profiles { get; set; }

        /// <summary>
        /// The composition profile that is currently selected.
        /// <para>
        /// Composition profiles determine what files are searched for and how many are allowed (or required).
        /// </para>
        /// </summary>
        public ICompositionProfile? SelectedProfile { get; set; }

        /// <summary>
        /// True, if segment menu is open.
        /// </summary>
        public bool AddSegmentMenuOpen { get; set; }

        /// <summary>
        /// True, is profile menu is open.
        /// </summary>
        public bool AddProfileMenuOpen { get; set; }

        /// <summary>
        /// Whether to show the contents of the profile.
        /// <para>
        /// Is bound to from the view.
        /// </para>
        /// </summary>
        public bool ProfileContentShow { get; }
    }
}
