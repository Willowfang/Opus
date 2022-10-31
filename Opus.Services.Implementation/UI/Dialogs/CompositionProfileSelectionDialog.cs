using Opus.Services.Data.Composition;
using System.Collections.Generic;

namespace Opus.Services.Implementation.UI.Dialogs
{
    /// <summary>
    /// Dialog for selecting the applicable composition profile.
    /// </summary>
    public class CompositionProfileSelectionDialog : DialogBase
    {
        /// <summary>
        /// All the profiles up for selection.
        /// </summary>
        public IList<ICompositionProfile> Profiles { get; }

        private ICompositionProfile? selectedProfile;

        /// <summary>
        /// Currently selected profile.
        /// </summary>
        public ICompositionProfile? SelectedProfile
        {
            get => selectedProfile;
            set => SetProperty(ref selectedProfile, value);
        }

        /// <summary>
        /// Create a new dialog for choosing the correct profile.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="profiles">All the profiles.</param>
        public CompositionProfileSelectionDialog(
            string dialogTitle,
            IList<ICompositionProfile> profiles
        ) : base(dialogTitle)
        {
            Profiles = profiles;
        }
    }
}
