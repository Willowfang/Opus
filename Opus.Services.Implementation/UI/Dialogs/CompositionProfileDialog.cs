using Opus.Services.Data;
using Opus.Services.Implementation.Data;
using Opus.Services.Implementation.UI;
using Opus.Services.UI;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class CompositionProfileDialog : DialogBase, IDialog
    {
        public IList<ICompositionProfile> Profiles { get; }
        public ICompositionProfile CurrentProfile { get; }
        public bool IsNewProfile { get; }

        private string? nameHelper;
        public string? NameHelper
        {
            get => nameHelper;
            set => SetProperty(ref nameHelper, value);
        }
        public string Name
        {
            get => CurrentProfile.ProfileName;
            set
            {
                CurrentProfile.ProfileName = value;
                NameHelper = null;
            }
        }

        public CompositionProfileDialog(IList<ICompositionProfile> profiles)
        {
            Profiles = profiles;
            IsNewProfile = true;
            CurrentProfile = new CompositionProfile(new ReorderCollection<ICompositionSegment>());
        }

        public CompositionProfileDialog(IList<ICompositionProfile> profiles, ICompositionProfile current)
        {
            Profiles = profiles;
            IsNewProfile = false;

            var copyCurrent = new CompositionProfile(current.Segments);
            copyCurrent.Id = current.Id;
            copyCurrent.IsEditable = current.IsEditable;
            copyCurrent.ProfileName = current.ProfileName;
            CurrentProfile = copyCurrent;
        }
        protected override void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(CurrentProfile.ProfileName))
            {
                NameHelper = Resources.Labels.CompositionProfileDialog_NameEmpty;
                return;
            }

            if (IsNewProfile)
            {
                if (Profiles.Any(x => x.ProfileName.ToLower() == CurrentProfile.ProfileName.ToLower()))
                {
                    NameHelper = Resources.Labels.CompositionProfileDialog_NameExists;
                    return;
                }
            }

            base.ExecuteSave();
        }
    }
}
