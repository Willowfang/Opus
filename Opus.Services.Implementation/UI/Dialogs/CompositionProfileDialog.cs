using Opus.Services.Data.Composition;
using Opus.Services.Implementation.Data;
using Opus.Services.Implementation.UI;
using Opus.Services.UI;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class CompositionProfileDialog : DialogBase, IDialog, IDataErrorInfo
    {
        private string? originalProfileName;

        public IList<ICompositionProfile> Profiles { get; }

        private bool suppressError;
        public bool SuppressError
        {
            get => suppressError;
            set
            {
                SetProperty(ref suppressError, value);
            }
        }

        private string? profileName;
        public string? ProfileName
        {
            get => profileName;
            set
            {
                SetProperty(ref profileName, value);
            }
        }

        private bool addPageNumbers;
        public bool AddPageNumbers
        {
            get => addPageNumbers;
            set => SetProperty(ref addPageNumbers, value);
        }

        public CompositionProfileDialog(string dialogTitle, IList<ICompositionProfile> profiles)
            : base(dialogTitle)
        {
            Profiles = profiles;
            originalProfileName = null;
        }

        public CompositionProfileDialog(string dialogTitle, string originalName,
            IList<ICompositionProfile> profiles) : this(dialogTitle, profiles)
        {
            this.originalProfileName = originalName;
        }

        public string? Error
        {
            get => null;
        }

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
                    if (Profiles.Any(x => x.ProfileName == ProfileName) &&
                        ProfileName != originalProfileName)
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
