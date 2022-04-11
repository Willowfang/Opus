using CX.LoggingLib;
using Opus.Services.Data.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.UI.Dialogs
{
    public class CompositionProfileSelectionDialog : DialogBase
    {
        public IList<ICompositionProfile> Profiles { get; }

        private ICompositionProfile? selectedProfile;
        public ICompositionProfile? SelectedProfile
        {
            get => selectedProfile;
            set => SetProperty(ref selectedProfile, value);
        }

        public CompositionProfileSelectionDialog(string dialogTitle,
            IList<ICompositionProfile> profiles)
            : base(dialogTitle) 
        {
            Profiles = profiles;
        }
    }
}
