using AsyncAwaitBestPractices.MVVM;
using CX.PdfLib.Services;
using Opus.Core.Base;
using Opus.Core.Constants;
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
    public class CompositionViewModel : ViewModelBase, INavigationTarget
    {
        /// <summary>
        /// Common event handling service
        /// </summary>
        private IEventAggregator aggregator;
        /// <summary>
        /// Service for performing manipulations on a pdf-file
        /// </summary>
        private IManipulator manipulator;
        /// <summary>
        /// Service handling user input for file and directory paths
        /// </summary>
        private IPathSelection input;
        /// <summary>
        /// Composition-specific configuration
        /// </summary>
        private IConfiguration configuration;
        /// <summary>
        /// Common service for dialog control
        /// </summary>
        private IDialogAssist dialogAssist;
        private ICompositionOptions options;
        private IComposerFactory composerFactory;

        /// <summary>
        /// All profiles stored in the database. Composition profiles
        /// determine what files are to be added and in what order, as well as 
        /// their corresponding info for adding bookmarks.
        /// </summary>
        public ObservableCollection<ICompositionProfile> Profiles { get; }

        /// <summary>
        /// The profile that is currently selected
        /// </summary>
        private ICompositionProfile selectedProfile;
        public ICompositionProfile SelectedProfile
        {
            get => selectedProfile;
            set
            {
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
        public bool AddSegmentMenuOpen
        {
            get => addSegmentMenuOpen;
            set => SetProperty(ref addSegmentMenuOpen, value);
        }

        private bool addProfileMenuOpen;
        public bool AddProfileMenuOpen
        {
            get => addProfileMenuOpen;
            set => SetProperty(ref addProfileMenuOpen, value);
        }

        public CompositionViewModel(IEventAggregator aggregator,
            IPathSelection input, IConfiguration configuration, INavigationTargetRegistry navReg,
            IDialogAssist dialogAssist, ICompositionOptions options, IManipulator manipulator,
            IComposerFactory composerFactory)
        {
            this.aggregator = aggregator;
            this.manipulator = manipulator;
            this.input = input;
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.options = options;
            this.composerFactory = composerFactory;

            IList<ICompositionProfile> profs = options.GetProfiles() ?? new List<ICompositionProfile>();
            Profiles = new ObservableCollection<ICompositionProfile>(profs);
            navReg.AddTarget(SchemeNames.COMPOSE, this);

            SelectedProfile = Profiles.FirstOrDefault(x => x.Id == configuration.DefaultProfile);
        }

        /// <summary>
        /// When the view associated with this view model is active and showing, subscribe to receive
        /// notifications of directory selection.
        /// </summary>
        SubscriptionToken directorySelectedSubscription;
        public void OnArrival()
        {
            directorySelectedSubscription = aggregator.GetEvent<DirectorySelectedEvent>().Subscribe(DirectorySelected);
        }
        public void WhenLeaving()
        {
            aggregator.GetEvent<DirectorySelectedEvent>().Unsubscribe(directorySelectedSubscription);
        }

        private async void DirectorySelected(string path)
        {
            await ExecuteComposition(path);
        }

        private void CollectionReordered(object sender, CollectionReorderedEventArgs e)
        {
            options.SaveProfile(SelectedProfile);
        }

        private DelegateCommand editable;
        public DelegateCommand Editable =>
            editable ??= new DelegateCommand(ExecuteEditable);

        private void ExecuteEditable()
        {
            SelectedProfile.IsEditable = !SelectedProfile.IsEditable;
            options.SaveProfile(SelectedProfile);
        }

        #region COMMANDS

        private DelegateCommand openSegmentMenu;
        public DelegateCommand OpenSegmentMenu =>
            openSegmentMenu ??= new DelegateCommand(() => AddSegmentMenuOpen = !AddSegmentMenuOpen);

        private DelegateCommand openProfileMenu;
        public DelegateCommand OpenProfileMenu =>
            openProfileMenu ??= new DelegateCommand(() => AddProfileMenuOpen = !AddSegmentMenuOpen);

        private IAsyncCommand editProfile;
        public IAsyncCommand EditProfile =>
            editProfile ??= new AsyncCommand(ExecuteEditProfile);
        private async Task ExecuteEditProfile()
        {
            CompositionProfileDialog dialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.EditTitle, SelectedProfile.ProfileName, 
                Profiles.ToList())
            {
                ProfileName = SelectedProfile.ProfileName,
                AddPageNumbers = SelectedProfile.AddPageNumbers
            };

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled) return;

            SelectedProfile.AddPageNumbers = dialog.AddPageNumbers;
            SelectedProfile.ProfileName = dialog.ProfileName;

            options.SaveProfile(SelectedProfile);
        }

        private IAsyncCommand addProfile;
        public IAsyncCommand AddProfile =>
            addProfile ??= new AsyncCommand(ExecuteAddProfile);
        private async Task ExecuteAddProfile()
        {
            CompositionProfileDialog dialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.NewTitle, Profiles.ToList())
            {
                AddPageNumbers = true
            };

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled) return;

            ICompositionProfile profile = options.CreateProfile(dialog.ProfileName,
                dialog.AddPageNumbers, true);

            SaveAddAndSelect(profile);
        }

        private IAsyncCommand deleteProfile;
        public IAsyncCommand DeleteProfile =>
            deleteProfile ??= new AsyncCommand(ExecuteDeleteProfile);
        private async Task ExecuteDeleteProfile()
        {
            ConfirmationDialog dialog = new ConfirmationDialog(Resources.Labels.Dialogs.Confirmation.DeleteCompositionProfileTitle,
                Resources.Messages.Composition.ProfileDeleteConfirmation);

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled) return;

            options.DeleteProfile(SelectedProfile);
            Profiles.Remove(SelectedProfile);
        }

        private IAsyncCommand copyProfile;
        public IAsyncCommand CopyProfile =>
            copyProfile ??= new AsyncCommand(ExecuteCopyProfile);
        private async Task ExecuteCopyProfile()
        {
            CompositionProfileDialog dialog = new CompositionProfileDialog(
                Resources.Labels.Dialogs.CompositionProfile.NewTitle, Profiles.ToList())
            {
                ProfileName = $"{SelectedProfile.ProfileName} ({Resources.Labels.General.Copy})",
                AddPageNumbers = SelectedProfile.AddPageNumbers
            };

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled) return;

            ICompositionProfile profile = options.CreateProfile(dialog.ProfileName,
                dialog.AddPageNumbers, true, SelectedProfile.Segments.ToList());

            SaveAddAndSelect(profile);
        }

        private IAsyncCommand importProfile;
        public IAsyncCommand ImportProfile =>
            importProfile ??= new AsyncCommand(ExecuteImportProfile);
        private async Task ExecuteImportProfile()
        {
            string filePath = input.OpenFile(Resources.UserInput.Descriptions.SelectOpenFile,
                FileType.Profile);

            if (filePath is null) return;

            ICompositionProfile profile;

            try
            {
                profile = options.ImportProfile(filePath);
                profile.IsEditable = true;
                profile.Id = Guid.NewGuid();
            }
            catch (ArgumentException)
            {
                MessageDialog messageDialog = new MessageDialog(Resources.Labels.General.Error,
                    string.Format(Resources.Messages.Composition.ProfileWrongExtension, 
                    Resources.Files.FileExtensions.Profile));
                await dialogAssist.Show(messageDialog);
                return;
            }
            catch (IOException)
            {
                MessageDialog messageDialog = new MessageDialog(Resources.Labels.General.Error,
                    Resources.Messages.Composition.ProfileImportFailed);
                await dialogAssist.Show(messageDialog);
                return;
            }

            if (Profiles.Any(x => x.ProfileName == profile.ProfileName))
            {
                ConfirmationDialog confirmationDialog = new ConfirmationDialog(
                    Resources.Labels.Dialogs.Confirmation.CompositionImportProfileExists,
                    Resources.Messages.Composition.ProfileImportExists);
                await dialogAssist.Show(confirmationDialog);

                if (confirmationDialog.IsCanceled) return;

                CompositionProfileDialog profileDialog = new CompositionProfileDialog(
                    Resources.Labels.Dialogs.CompositionProfile.ImportTitle,
                    Profiles.ToList())
                {
                    AddPageNumbers = profile.AddPageNumbers
                };

                await dialogAssist.Show(profileDialog);

                if (profileDialog.IsCanceled) return;

                profile.ProfileName = profileDialog.ProfileName;
                profile.AddPageNumbers = profileDialog.AddPageNumbers;
            }

            SaveAddAndSelect(profile);

            MessageDialog successDialog = new MessageDialog(Resources.Labels.General.Notification,
                Resources.Messages.Composition.ProfileImportSuccess);
            await dialogAssist.Show(successDialog);
            return;
        }

        private IAsyncCommand exportProfile;
        public IAsyncCommand ExportProfile =>
            exportProfile ??= new AsyncCommand(ExecuteExportProfile);
        private async Task ExecuteExportProfile()
        {
            string filePath = input.SaveFile(Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.Profile, SelectedProfile.ProfileName + Resources.Files.FileExtensions.Profile);

            if (filePath is null) return;

            try
            {
                bool success = options.ExportProfile(SelectedProfile, filePath);
                if (success == false)
                {
                    MessageDialog messageDialog = new MessageDialog(Resources.Labels.General.Error,
                        Resources.Messages.Composition.ProfileExportFailed);

                    await dialogAssist.Show(messageDialog);
                    return;
                }
            }
            catch (ArgumentException)
            {
                MessageDialog messageDialog = new MessageDialog(Resources.Labels.General.Error,
                    string.Format(Resources.Messages.Composition.ProfileWrongExtension,
                    Resources.Files.FileExtensions.Profile));
                await dialogAssist.Show(messageDialog);
                return;
            }

            MessageDialog successDialog = new MessageDialog(Resources.Labels.General.Notification,
                Resources.Messages.Composition.ProfileExportSuccess);
            await dialogAssist.Show(successDialog);
            return;
        }

        private IAsyncCommand addFileSegment;
        public IAsyncCommand AddFileSegment =>
            addFileSegment ??= new AsyncCommand(ExecuteAddFileSegment);
        private async Task ExecuteAddFileSegment()
        {
            CompositionFileSegmentDialog dialog = new CompositionFileSegmentDialog(
                Resources.Labels.Dialogs.CompositionFileSegment.NewTitle);

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled) return;

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
        }

        private IAsyncCommand editSegment;
        public IAsyncCommand EditSegment =>
            editSegment ??= new AsyncCommand(ExecuteEditSegment);
        private async Task ExecuteEditSegment()
        {
            if (SelectedProfile.Segments.SelectedItem is ICompositionFile fileSegment)
            {
                CompositionFileSegmentDialog dialog = new CompositionFileSegmentDialog(
                Resources.Labels.Dialogs.CompositionFileSegment.EditTitle);

                dialog.SegmentName = fileSegment.SegmentName;
                dialog.NameFromFile = fileSegment.NameFromFile;
                dialog.SearchTerm = fileSegment.SearchExpressionString;
                dialog.ToRemove = fileSegment.IgnoreExpressionString;
                dialog.MinCount = fileSegment.MinCount;
                dialog.MaxCount = fileSegment.MaxCount;
                dialog.Example = fileSegment.Example;

                await dialogAssist.Show(dialog);

                if (dialog.IsCanceled) return;

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
                    Resources.Labels.Dialogs.CompositionFileSegment.EditTitle);
                
                dialog.SegmentName = titleSegment.SegmentName;

                await dialogAssist.Show(dialog);

                if (dialog.IsCanceled) return;

                titleSegment.SegmentName = dialog.SegmentName;

                options.SaveProfile(SelectedProfile);
            }

        }

        private IAsyncCommand addTitleSegment;
        public IAsyncCommand AddTitleSegment =>
            addTitleSegment ??= new AsyncCommand(ExecuteAddTitleSegment);
        private async Task ExecuteAddTitleSegment()
        {
            CompositionTitleSegmentDialog dialog = new CompositionTitleSegmentDialog(
                Resources.Labels.Dialogs.CompositionFileSegment.NewTitle);

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled) return;

            ICompositionTitle segment = options.CreateTitleSegment(dialog.SegmentName);
            SelectedProfile.Segments.Add(segment);
            options.SaveProfile(SelectedProfile);
        }

        private IAsyncCommand deleteSegment;
        public IAsyncCommand DeleteSegment =>
            deleteSegment ??= new AsyncCommand(ExecuteDeleteSegment);
        private async Task ExecuteDeleteSegment()
        {
            ConfirmationDialog dialog = new ConfirmationDialog(Resources.Labels.Dialogs.Confirmation.DeleteCompositionSegmentTitle,
                Resources.Messages.Composition.SegmentDeleteConfirmation);

            await dialogAssist.Show(dialog);

            if (dialog.IsCanceled) return;

            SelectedProfile.Segments.Remove(SelectedProfile.Segments.SelectedItem);
            options.SaveProfile(SelectedProfile);
        }

        private async Task ExecuteComposition(string directory)
        {
            IComposer composer = composerFactory.Create();
            await composer.Compose(directory, SelectedProfile, configuration.CompositionDeleteConverted,
                configuration.CompositionSearchSubDirectories);
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
