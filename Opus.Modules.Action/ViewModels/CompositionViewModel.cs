using AsyncAwaitBestPractices.MVVM;
using CX.LoggingLib;
using CX.PdfLib.Services;
using Opus.Core.Base;
using Opus.Values;
using Opus.Events;
using Opus.Services.Configuration;
using Opus.Services.Data;
using Opus.Services.Data.Composition;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Input;
using Opus.Services.UI;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// View model for composition views. Handles actions dealing with document composition.
    /// </summary>
    public class CompositionViewModel
        : ViewModelBaseLogging<CompositionViewModel>,
            INavigationTarget
    {
        #region DI services
        private IEventAggregator aggregator;
        private IPathSelection input;
        private IConfiguration configuration;
        private IDialogAssist dialogAssist;
        private ICompositionOptions options;
        private IComposer composer;
        #endregion

        #region Fields and properties
        /// <summary>
        /// All profiles stored in the database. Composition profiles
        /// determine what files are to be added and in what order, as well as
        /// their corresponding info for adding bookmarks.
        /// </summary>
        public ObservableCollection<ICompositionProfile> Profiles { get; }

        private ICompositionProfile selectedProfile;

        /// <summary>
        /// The composition profile that is currently selected.
        /// <para>
        /// Composition profiles determine what files are searched for and how many are allowed (or required).
        /// </para>
        /// </summary>
        public ICompositionProfile SelectedProfile
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
        /// Is the segment addition menu open?
        /// <para>
        /// Is bound to from the view.
        /// </para>
        /// </summary>
        public bool AddSegmentMenuOpen
        {
            get => addSegmentMenuOpen;
            set => SetProperty(ref addSegmentMenuOpen, value);
        }

        private bool addProfileMenuOpen;

        /// <summary>
        /// Is profile addition menu open?
        /// <para>
        /// Is bound to from the view.
        /// </para>
        /// </summary>
        public bool AddProfileMenuOpen
        {
            get => addProfileMenuOpen;
            set => SetProperty(ref addProfileMenuOpen, value);
        }

        /// <summary>
        /// Whether to show the contents of the profile.
        /// <para>
        /// Is bound to from the view.
        /// </para>
        /// </summary>
        public bool ProfileContentShow
        {
            get => true;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new viewmodel for the composition view.
        /// </summary>
        /// <param name="aggregator">Common event handling service.</param>
        /// <param name="input">Service for user filepath input.</param>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="navReg">Navigation registry service for registering this viewModel.</param>
        /// <param name="dialogAssist">Service showing and otherwise handling dialogs.</param>
        /// <param name="options">Composition options as their own instance.</param>
        /// <param name="composer">Composition services.</param>
        /// <param name="logbook">Logging service.</param>
        public CompositionViewModel(
            IEventAggregator aggregator,
            IPathSelection input,
            IConfiguration configuration,
            INavigationTargetRegistry navReg,
            IDialogAssist dialogAssist,
            ICompositionOptions options,
            IComposer composer,
            ILogbook logbook
        ) : base(logbook)
        {
            // Assign DI services
            this.aggregator = aggregator;
            this.input = input;
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.options = options;
            this.composer = composer;

            // Load composition profiles from options.
            IList<ICompositionProfile> profs =
                options.GetProfiles() ?? new List<ICompositionProfile>();
            Profiles = new ObservableCollection<ICompositionProfile>(profs);
            navReg.AddTarget(SchemeNames.COMPOSE, this);

            SelectedProfile = Profiles.FirstOrDefault(x => x.Id == configuration.DefaultProfile);
        }
        #endregion

        #region INavigationTarget implementation
        /// <summary>
        /// When the view associated with this view model is active and shown, subscribe to receive
        /// notifications of directory selection. This token is stored to cancel said description.
        /// </summary>
        SubscriptionToken directorySelectedSubscription;

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>.
        /// <para>
        /// Actions to take when this viewmodel is navigated to.
        /// </para>
        /// </summary>
        public void OnArrival()
        {
            // Subscribe to directory selection events (to start composing).

            directorySelectedSubscription = aggregator
                .GetEvent<DirectorySelectedEvent>()
                .Subscribe(DirectorySelected);

            logbook.Write(
                $"{this} subscribed to {nameof(DirectorySelectedEvent)}.",
                LogLevel.Debug
            );
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>.
        /// <para>
        /// Actions to take when this viewmodel is navigated from.
        /// </para>
        /// </summary>
        public void WhenLeaving()
        {
            // Unsubscribe from directory selection event.

            aggregator
                .GetEvent<DirectorySelectedEvent>()
                .Unsubscribe(directorySelectedSubscription);

            logbook.Write(
                $"{this} unsubscribed from {nameof(DirectorySelectedEvent)}.",
                LogLevel.Debug
            );
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>.
        /// <para>
        /// Actions to take when this reset button is presseed.
        /// </para>
        /// <para>
        /// Composition view model has no state that would be reset, when button is pressed. So do nothing.
        /// </para>
        /// </summary>
        public void Reset() { }

        /// <summary>
        /// Action to execute when a directory is selected.
        /// <para>
        /// Will immediately run composition.
        /// </para>
        /// </summary>
        /// <param name="path">Path of the selected directory.</param>
        protected async void DirectorySelected(string path)
        {
            await ExecuteComposition(path);
        }
        #endregion

        #region Event handlers
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
            options.SaveProfile(SelectedProfile);
        }
        #endregion

        #region Commands

        private DelegateCommand editableCommand;

        /// <summary>
        /// Command to change the editability of a profile.
        /// </summary>
        public DelegateCommand EditableCommand =>
            editableCommand ??= new DelegateCommand(ExecuteEditable);

        /// <summary>
        /// Execution method for editability command, see <see cref="EditableCommand"/>.
        /// <para>
        /// Makes the profile (un)editable and saves it.
        /// </para>
        /// </summary>
        protected void ExecuteEditable()
        {
            SelectedProfile.IsEditable = !SelectedProfile.IsEditable;
            options.SaveProfile(SelectedProfile);
        }

        private DelegateCommand openSegmentMenuCommand;

        /// <summary>
        /// Command for opening the segment menu.
        /// <para>
        /// The relevant bool is reversed.
        /// </para>
        /// </summary>
        public DelegateCommand OpenSegmentMenuCommand =>
            openSegmentMenuCommand ??= new DelegateCommand(
                () => AddSegmentMenuOpen = !AddSegmentMenuOpen
            );

        private DelegateCommand openProfileMenuCommand;

        /// <summary>
        /// Command for opening the profile menu.
        /// <para>
        /// The relevant bool is reversed.
        /// </para>
        /// </summary>
        public DelegateCommand OpenProfileMenuCommand =>
            openProfileMenuCommand ??= new DelegateCommand(
                () => AddProfileMenuOpen = !AddSegmentMenuOpen
            );

        private IAsyncCommand editProfileCommand;

        /// <summary>
        /// Command for editing the current profile.
        /// </summary>
        public IAsyncCommand EditProfileCommand =>
            editProfileCommand ??= new AsyncCommand(ExecuteEditProfile);

        /// <summary>
        /// Execution method for profile editing command, see <see cref="EditProfileCommand"/>.
        /// <para>
        /// Open a dialog for editing the profile.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        protected async Task ExecuteEditProfile()
        {
            // Create a dialog for editing the profile and show it.

            CompositionProfileDialog dialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.EditTitle,
                SelectedProfile.ProfileName,
                Profiles.ToList()
            )
            {
                ProfileName = SelectedProfile.ProfileName,
                AddPageNumbers = SelectedProfile.AddPageNumbers
            };

            await dialogAssist.Show(dialog);

            // If cancelled, just return.

            if (dialog.IsCanceled)
                return;

            // Save the edited profile.

            SelectedProfile.AddPageNumbers = dialog.AddPageNumbers;
            SelectedProfile.ProfileName = dialog.ProfileName;

            options.SaveProfile(SelectedProfile);

            logbook.Write(
                $"{nameof(ICompositionProfile)} '{selectedProfile.ProfileName}' edited.",
                LogLevel.Information
            );
        }

        private IAsyncCommand addProfileCommand;

        /// <summary>
        /// Command for adding a new profile.
        /// </summary>
        public IAsyncCommand AddProfileCommand =>
            addProfileCommand ??= new AsyncCommand(ExecuteAddProfile);

        /// <summary>
        /// Execution method for profile adding command, see <see cref="AddProfileCommand"/>.
        /// <para>
        /// Opens a dialog for entering info on the new profile.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        protected async Task ExecuteAddProfile()
        {
            // Create a new dialog and show it to the user.

            CompositionProfileDialog dialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.NewTitle,
                Profiles.ToList()
            )
            {
                AddPageNumbers = true
            };

            await dialogAssist.Show(dialog);

            // If canceled, just return.

            if (dialog.IsCanceled)
                return;

            // Create a new profile, save it to the database and immediately select it.

            ICompositionProfile profile = options.CreateProfile(
                dialog.ProfileName,
                dialog.AddPageNumbers,
                true
            );

            SaveAddAndSelect(profile);

            logbook.Write(
                $"{nameof(ICompositionProfile)} '{profile.ProfileName}' added.",
                LogLevel.Information
            );
        }

        private IAsyncCommand deleteProfileCommand;

        /// <summary>
        /// Command for deleting the selected profile.
        /// </summary>
        public IAsyncCommand DeleteProfileCommand =>
            deleteProfileCommand ??= new AsyncCommand(ExecuteDeleteProfile);

        /// <summary>
        /// Execution method for profile deletion command, <see cref="DeleteProfileCommand"/>.
        /// <para>
        /// Confirms deletion from the user and deletes the profile, if confirmation was received.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        protected async Task ExecuteDeleteProfile()
        {
            // Create confirmation dialog and show it to the user.

            ConfirmationDialog dialog = new ConfirmationDialog(
                Resources.Labels.Dialogs.Confirmation.DeleteCompositionProfileTitle,
                Resources.Messages.Composition.ProfileDeleteConfirmation
            );

            await dialogAssist.Show(dialog);

            // If cancelled, just return.

            if (dialog.IsCanceled)
                return;

            // Delete selected profile from the database.

            options.DeleteProfile(SelectedProfile);

            logbook.Write(
                $"{nameof(ICompositionProfile)} '{SelectedProfile.ProfileName}' deleted.",
                LogLevel.Information
            );

            // Remove profile from the current list.

            Profiles.Remove(SelectedProfile);
        }

        private IAsyncCommand copyProfileCommand;

        /// <summary>
        /// Command for copying the current profile.
        /// </summary>
        public IAsyncCommand CopyProfileCommand =>
            copyProfileCommand ??= new AsyncCommand(ExecuteCopyProfile);

        /// <summary>
        /// Execution method for profile copying command, see <see cref="CopyProfileCommand"/>.
        /// <para>
        /// Copies the profile as a new profile.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        protected async Task ExecuteCopyProfile()
        {
            // Create a dialog for entering info on the copied profile (i.e. the name of the copied
            // profile cannot be identical with the original one).

            CompositionProfileDialog dialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.NewTitle,
                Profiles.ToList()
            )
            {
                ProfileName = $"{SelectedProfile.ProfileName} ({Resources.Labels.General.Copy})",
                AddPageNumbers = SelectedProfile.AddPageNumbers
            };

            string originalName = SelectedProfile.ProfileName;

            await dialogAssist.Show(dialog);

            // If cancelled, just return.

            if (dialog.IsCanceled)
                return;

            // Create a new profile with copied (and modified) info.

            ICompositionProfile profile = options.CreateProfile(
                dialog.ProfileName,
                dialog.AddPageNumbers,
                true,
                SelectedProfile.Segments.ToList()
            );

            SaveAddAndSelect(profile);

            logbook.Write(
                $"{nameof(ICompositionProfile)} '{originalName}' copied to '{profile.ProfileName}'.",
                LogLevel.Information
            );
        }

        private IAsyncCommand importProfileCommand;

        /// <summary>
        /// Command for importing a profile from file.
        /// </summary>
        public IAsyncCommand ImportProfileCommand =>
            importProfileCommand ??= new AsyncCommand(ExecuteImportProfile);

        /// <summary>
        /// Execution method for import profile command, see <see cref="ImportProfileCommand"/>.
        /// <para>
        /// Opens the importable profile file, reads it, imports the profile if possible and closes the file.muistio
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        protected async Task ExecuteImportProfile()
        {
            // Ask the user for filepath to the profile to import.

            string filePath = input.OpenFile(
                Resources.UserInput.Descriptions.SelectOpenFile,
                FileType.Profile
            );

            if (filePath is null)
                return;

            ICompositionProfile profile;

            // Import the profile or show error message, if there is an exception.

            try
            {
                profile = options.ImportProfile(filePath);
                profile.IsEditable = true;
                profile.Id = Guid.NewGuid();
            }
            catch (ArgumentException ex) when (ex.Message == filePath)
            {
                MessageDialog messageDialog = new MessageDialog(
                    Resources.Labels.General.Error,
                    string.Format(
                        Resources.Messages.Composition.ProfileWrongExtension,
                        Resources.Files.FileExtensions.Profile
                    )
                );
                await dialogAssist.Show(messageDialog);
                return;
            }
            catch (Exception)
            {
                MessageDialog messageDialog = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.Composition.ProfileImportFailed
                );
                await dialogAssist.Show(messageDialog);
                return;
            }

            // If a profile with the same name already exists, confirm whether that profile
            // should be overwritten (or the new profile renamed).

            if (Profiles.Any(x => x.ProfileName == profile.ProfileName))
            {
                ConfirmationDialog confirmationDialog = new ConfirmationDialog(
                    Resources.Labels.Dialogs.Confirmation.CompositionImportProfileExists,
                    Resources.Messages.Composition.ProfileImportExists
                );
                await dialogAssist.Show(confirmationDialog);

                if (confirmationDialog.IsCanceled)
                {
                    logbook.Write(
                        $"Cancellation requested at {nameof(IDialog)} '{confirmationDialog.DialogTitle}'.",
                        LogLevel.Information
                    );
                    return;
                }

                CompositionProfileDialog profileDialog = new CompositionProfileDialog(
                    Resources.Labels.Dialogs.CompositionProfile.ImportTitle,
                    Profiles.ToList()
                )
                {
                    AddPageNumbers = profile.AddPageNumbers
                };

                await dialogAssist.Show(profileDialog);

                if (profileDialog.IsCanceled)
                {
                    logbook.Write(
                        $"Cancellation requested at {nameof(IDialog)} '{profileDialog.DialogTitle}'.",
                        LogLevel.Information
                    );
                    return;
                }

                profile.ProfileName = profileDialog.ProfileName;
                profile.AddPageNumbers = profileDialog.AddPageNumbers;
            }

            // Save the profile and select it.

            SaveAddAndSelect(profile);

            logbook.Write(
                $"{nameof(ICompositionProfile)} '{profile.ProfileName}' imported from {filePath}.",
                LogLevel.Information
            );

            MessageDialog successDialog = new MessageDialog(
                Resources.Labels.General.Notification,
                Resources.Messages.Composition.ProfileImportSuccess
            );
            await dialogAssist.Show(successDialog);
            return;
        }

        private IAsyncCommand exportProfileCommand;

        /// <summary>
        /// A command to export the selected profile.
        /// </summary>
        public IAsyncCommand ExportProfileCommand =>
            exportProfileCommand ??= new AsyncCommand(ExecuteExportProfile);

        /// <summary>
        /// Execution method for profile export command.
        /// <para>
        /// Exports the current profile as .opusprofile -file.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        protected async Task ExecuteExportProfile()
        {
            string filePath = input.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.Profile,
                SelectedProfile.ProfileName + Resources.Files.FileExtensions.Profile
            );

            if (filePath is null)
                return;

            try
            {
                bool success = options.ExportProfile(SelectedProfile, filePath);
                if (success == false)
                {
                    MessageDialog messageDialog = new MessageDialog(
                        Resources.Labels.General.Error,
                        Resources.Messages.Composition.ProfileExportFailed
                    );

                    await dialogAssist.Show(messageDialog);
                    return;
                }
            }
            catch (ArgumentException)
            {
                MessageDialog messageDialog = new MessageDialog(
                    Resources.Labels.General.Error,
                    string.Format(
                        Resources.Messages.Composition.ProfileWrongExtension,
                        Resources.Files.FileExtensions.Profile
                    )
                );
                await dialogAssist.Show(messageDialog);
                return;
            }

            logbook.Write(
                $"{nameof(ICompositionProfile)} '{SelectedProfile.ProfileName}' exported to {filePath}.",
                LogLevel.Information
            );

            MessageDialog successDialog = new MessageDialog(
                Resources.Labels.General.Notification,
                Resources.Messages.Composition.ProfileExportSuccess
            );
            await dialogAssist.Show(successDialog);
            return;
        }

        private IAsyncCommand addFileSegment;
        public IAsyncCommand AddFileSegment =>
            addFileSegment ??= new AsyncCommand(ExecuteAddFileSegment);

        private async Task ExecuteAddFileSegment()
        {
            CompositionFileSegmentDialog dialog = new CompositionFileSegmentDialog(
                Resources.Labels.Dialogs.CompositionFileSegment.NewTitle
            );

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
                return;

            ICompositionFile segment = options.CreateFileSegment(dialog.SegmentName);
            segment.NameFromFile = dialog.NameFromFile;
            segment.SearchExpressionString = dialog.SearchTerm;
            if (dialog.ToRemove != null)
            {
                segment.IgnoreExpressionString = dialog.ToRemove;
            }
            segment.MinCount = dialog.MinCount;
            segment.MaxCount = dialog.MaxCount;
            segment.Example = dialog.Example;

            SelectedProfile.Segments.Add(segment);
            options.SaveProfile(SelectedProfile);

            logbook.Write(
                $"{nameof(ICompositionFile)} '{segment.DisplayName}' added to {nameof(ICompositionProfile)} '{SelectedProfile.ProfileName}'.",
                LogLevel.Information
            );
        }

        private IAsyncCommand editSegment;
        public IAsyncCommand EditSegment => editSegment ??= new AsyncCommand(ExecuteEditSegment);

        private async Task ExecuteEditSegment()
        {
            if (SelectedProfile.Segments.SelectedItem is ICompositionFile fileSegment)
            {
                CompositionFileSegmentDialog dialog = new CompositionFileSegmentDialog(
                    Resources.Labels.Dialogs.CompositionFileSegment.EditTitle
                );

                dialog.SegmentName = fileSegment.SegmentName;
                dialog.NameFromFile = fileSegment.NameFromFile;
                dialog.SearchTerm = fileSegment.SearchExpressionString;
                dialog.ToRemove = fileSegment.IgnoreExpressionString;
                dialog.MinCount = fileSegment.MinCount;
                dialog.MaxCount = fileSegment.MaxCount;
                dialog.Example = fileSegment.Example;

                await dialogAssist.Show(dialog);

                if (dialog.IsCanceled)
                {
                    logbook.Write(
                        $"Cancellation requested at {nameof(IDialog)} '{dialog.DialogTitle}'.",
                        LogLevel.Information
                    );
                    return;
                }

                fileSegment.SegmentName = dialog.SegmentName;
                fileSegment.NameFromFile = dialog.NameFromFile;
                fileSegment.SearchExpressionString = dialog.SearchTerm;
                fileSegment.IgnoreExpressionString = dialog.ToRemove;
                fileSegment.MinCount = dialog.MinCount;
                fileSegment.MaxCount = dialog.MaxCount;
                fileSegment.Example = dialog.Example;

                options.SaveProfile(SelectedProfile);
            }
            else if (SelectedProfile.Segments.SelectedItem is ICompositionTitle titleSegment)
            {
                CompositionTitleSegmentDialog dialog = new CompositionTitleSegmentDialog(
                    Resources.Labels.Dialogs.CompositionFileSegment.EditTitle
                );

                dialog.SegmentName = titleSegment.SegmentName;

                await dialogAssist.Show(dialog);

                if (dialog.IsCanceled)
                {
                    logbook.Write(
                        $"Cancellation requested at {nameof(IDialog)} '{dialog.DialogTitle}'.",
                        LogLevel.Information
                    );
                    return;
                }

                titleSegment.SegmentName = dialog.SegmentName;

                options.SaveProfile(SelectedProfile);
            }

            logbook.Write(
                $"{nameof(ICompositionSegment)} '{SelectedProfile.Segments.SelectedItem.DisplayName}' edited.",
                LogLevel.Information
            );
        }

        private IAsyncCommand addTitleSegment;
        public IAsyncCommand AddTitleSegment =>
            addTitleSegment ??= new AsyncCommand(ExecuteAddTitleSegment);

        private async Task ExecuteAddTitleSegment()
        {
            CompositionTitleSegmentDialog dialog = new CompositionTitleSegmentDialog(
                Resources.Labels.Dialogs.CompositionFileSegment.NewTitle
            );

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(IDialog)} '{dialog.DialogTitle}'.",
                    LogLevel.Information
                );
                return;
            }

            ICompositionTitle segment = options.CreateTitleSegment(dialog.SegmentName);
            SelectedProfile.Segments.Add(segment);
            options.SaveProfile(SelectedProfile);

            logbook.Write(
                $"{nameof(ICompositionTitle)} '{segment.DisplayName}' added to {nameof(ICompositionProfile)} '{SelectedProfile.ProfileName}'",
                LogLevel.Information
            );
        }

        private IAsyncCommand deleteSegment;
        public IAsyncCommand DeleteSegment =>
            deleteSegment ??= new AsyncCommand(ExecuteDeleteSegment);

        private async Task ExecuteDeleteSegment()
        {
            ConfirmationDialog dialog = new ConfirmationDialog(
                Resources.Labels.Dialogs.Confirmation.DeleteCompositionSegmentTitle,
                Resources.Messages.Composition.SegmentDeleteConfirmation
            );

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(IDialog)} '{dialog.DialogTitle}'.",
                    LogLevel.Information
                );
                return;
            }

            string selectedName = SelectedProfile.Segments.SelectedItem.DisplayName;

            SelectedProfile.Segments.Remove(SelectedProfile.Segments.SelectedItem);
            options.SaveProfile(SelectedProfile);

            logbook.Write(
                $"{nameof(ICompositionSegment)} '{selectedName}' deleted from {nameof(ICompositionProfile)} '{SelectedProfile.ProfileName}'",
                LogLevel.Information
            );
        }

        private async Task ExecuteComposition(string directory)
        {
            logbook.Write($"Starting composition.", LogLevel.Information);

            await composer.Compose(
                directory,
                SelectedProfile,
                configuration.CompositionDeleteConverted,
                configuration.CompositionSearchSubDirectories
            );

            logbook.Write($"Composition finished.", LogLevel.Information);
        }

        #endregion

        private void SaveAddAndSelect(ICompositionProfile profile)
        {
            options.SaveProfile(profile);
            Profiles.Add(profile);
            SelectedProfile = profile;
        }
    }
}
