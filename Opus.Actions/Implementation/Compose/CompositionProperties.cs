using Opus.Actions.Services.Compose;
using Opus.Common.Collections;
using Opus.Common.Services.Configuration;
using Opus.Common.Services.Data.Composition;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Opus.Actions.Implementation.Compose
{
    /// <summary>
    /// Implementation for <see cref="ICompositionProperties"/>.
    /// </summary>
    public class CompositionProperties : BindableBase, ICompositionProperties
    {
        private readonly IConfiguration configuration;
        private readonly ICompositionOptions options;

        private ObservableCollection<ICompositionProfile> profiles;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ObservableCollection<ICompositionProfile> Profiles
        {
            get => profiles;
            set => SetProperty(ref profiles, value);
        }

        private ICompositionProfile? selectedProfile;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICompositionProfile? SelectedProfile
        {
            get => selectedProfile;
            set
            {
                // Set the return value and set the collection reordering event for
                // the segments of the currently selected profile.

                if (selectedProfile != null && selectedProfile.Segments != null)
                {
                    selectedProfile.Segments.CollectionReordered -= CollectionReordered;
                }

                SetProperty(ref selectedProfile, value);
                configuration.DefaultProfile = value != null ? value.Id : Guid.Empty;

                if (selectedProfile != null && selectedProfile.Segments != null)
                {
                    selectedProfile.Segments.CollectionReordered += CollectionReordered;
                }

                RaisePropertyChanged(nameof(SelectedProfile.Segments));
            }
        }

        private bool addSegmentMenuOpen;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool AddSegmentMenuOpen
        {
            get => addSegmentMenuOpen;
            set => SetProperty(ref addSegmentMenuOpen, value);
        }

        private bool addProfileMenuOpen;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool AddProfileMenuOpen
        {
            get => addProfileMenuOpen;
            set => SetProperty(ref addProfileMenuOpen, value);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool ProfileContentShow
        {
            get => true;
        }

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="configuration">Application configuration service.</param>
        /// <param name="options">Options for composition.</param>
        public CompositionProperties(
            IConfiguration configuration,
            ICompositionOptions options)
        {
            this.configuration = configuration;
            this.options = options;

            // Load composition profiles from options.
            IList<ICompositionProfile> profs =
                options.GetProfiles() ?? new List<ICompositionProfile>();

            profiles = new ObservableCollection<ICompositionProfile>(profs);

            SelectedProfile = Profiles.FirstOrDefault(x => x.Id == configuration.DefaultProfile);
        }

        /// <summary>
        /// Is performed when segments have been altered by the user.
        /// <para>
        /// Will save the profile.
        /// </para>
        /// </summary>
        /// <param name="sender">Sending collection.</param>
        /// <param name="e">Event arguments.</param>
        private void CollectionReordered(object sender, CollectionReorderedEventArgs e)
        {
            if (SelectedProfile != null) options.SaveProfile(SelectedProfile);
        }
    }
}
