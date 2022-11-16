using Opus.Common.Services.Data.Composition;
using Opus.Common.Services.Dialogs;
using System.ComponentModel;

namespace Opus.Common.Dialogs
{
    /// <summary>
    /// Dialog for adding or editing a composition profile.
    /// </summary>
    public class CompositionProfileDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private string? originalProfileName;

        /// <summary>
        /// A list of all existing profiles (to check for doubles).
        /// </summary>
        public IList<ICompositionProfile> Profiles { get; }

        private bool suppressError;

        /// <summary>
        /// If true, suppress validation erro messages.
        /// </summary>
        public bool SuppressError
        {
            get => suppressError;
            set { SetProperty(ref suppressError, value); }
        }

        private string? profileName;

        /// <summary>
        /// Name of this profile.
        /// </summary>
        public string? ProfileName
        {
            get => profileName;
            set { SetProperty(ref profileName, value); }
        }

        private bool addPageNumbers;

        /// <summary>
        /// If true, add page numbers to final product.
        /// </summary>
        public bool AddPageNumbers
        {
            get => addPageNumbers;
            set => SetProperty(ref addPageNumbers, value);
        }

        /// <summary>
        /// Create a new dialog for providing composition profile information.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="profiles">All the profiles.</param>
        public CompositionProfileDialog(string dialogTitle, IList<ICompositionProfile> profiles)
            : base(dialogTitle)
        {
            Profiles = profiles;
            originalProfileName = null;
        }

        /// <summary>
        /// Create a new dialog for providing composition profile information.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="originalName">Original name of the profile.</param>
        /// <param name="profiles">All the profiles.</param>
        public CompositionProfileDialog(
            string dialogTitle,
            string? originalName,
            IList<ICompositionProfile> profiles
        ) : this(dialogTitle, profiles)
        {
            originalProfileName = originalName;
        }

        /// <summary>
        /// Validation error. Always return null.
        /// </summary>
        public string Error
        {
            get => string.Empty;
        }

        /// <summary>
        /// Validation.
        /// </summary>
        /// <param name="propertyName">Property to validate.</param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == nameof(ProfileName))
                {
                    if (string.IsNullOrEmpty(ProfileName))
                    {
                        SuppressError = true;
                        return Resources.Validation.General.NameEmpty;
                    }
                    if (
                        Profiles.Any(x => x.ProfileName == ProfileName)
                        && ProfileName != originalProfileName
                    )
                    {
                        SuppressError = false;
                        return Resources.Validation.Composition.ProfileNameExists;
                    }
                }

                return string.Empty;
            }
        }
    }
}
