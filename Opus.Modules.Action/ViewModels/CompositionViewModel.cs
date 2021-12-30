using AsyncAwaitBestPractices.MVVM;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using Opus.Core.Base;
using Opus.Core.Constants;
using Opus.Events;
using Opus.Services.Configuration;
using Opus.Services.Data;
using Opus.Services.Implementation.Data;
using Opus.Services.Implementation.UI;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Input;
using Opus.Services.UI;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        /// <summary>
        /// Directory to search for composition files
        /// </summary>
        private string selectedDirectory;

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

        public CompositionViewModel(IEventAggregator aggregator,
            IPathSelection input, IConfiguration configuration, INavigationTargetRegistry navReg,
            IDialogAssist dialogAssist, ICompositionOptions options, IManipulator manipulator)
        {
            this.aggregator = aggregator;
            this.manipulator = manipulator;
            this.input = input;
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.options = options;

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

        private void DirectorySelected(string path)
        {
            selectedDirectory = path;
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
            openSegmentMenu ??= new DelegateCommand(() => AddSegmentMenuOpen = true);

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

            options.SaveProfile(profile);
            Profiles.Add(profile);
            SelectedProfile = profile;
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
            segment.SetSearchTerm(dialog.SearchTerm);
            if (dialog.ToRemove != null)
            {
                segment.SetToRemove(dialog.ToRemove);
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
                dialog.SearchTerm = fileSegment.SearchTerm.ToString();
                dialog.ToRemove = fileSegment.ToRemove.ToString();
                dialog.MinCount = fileSegment.MinCount;
                dialog.MaxCount = fileSegment.MaxCount;
                dialog.Example = fileSegment.Example;

                await dialogAssist.Show(dialog);

                if (dialog.IsCanceled) return;

                fileSegment.SegmentName = dialog.SegmentName;
                fileSegment.NameFromFile = dialog.NameFromFile;
                fileSegment.SetSearchTerm(dialog.SearchTerm);
                fileSegment.SetToRemove(dialog.ToRemove);
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

        #endregion

        private async Task Compose()
        {
            if (selectedDirectory == null)
            {
                await dialogAssist.Show(new MessageDialog(Resources.Labels.General.Error, 
                    Resources.Messages.Composition.FolderNotSelected));
                return;
            }

            // Get all pdf and Word files in selected directory and its subdirectories
            string[] allFiles = Directory.GetFiles(selectedDirectory, "*.*", SearchOption.AllDirectories)
                .Where(x => x.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || 
                x.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || 
                x.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // List of files and titles to merge
            List<IMergeInput> mergingInputs = GetInputs(allFiles);
        }

        private List<IMergeInput> GetInputs(string[] filePaths)
        {
            List<IMergeInput> inputs = new List<IMergeInput>();

            foreach (ICompositionSegment segment in SelectedProfile.Segments)
            {
                if (segment is ICompositionFile fileSegment)
                {
                    List<IFileEvaluationResult> found = EvaluateFilesAgainstSegment(fileSegment, filePaths);
                    List<IFileEvaluationResult> corrected = EvaluateCounts(fileSegment, found);

                    foreach (IFileEvaluationResult result in corrected)
                    {
                        string name = fileSegment.NameFromFile == true ? result.Name : fileSegment.SegmentName;
                        inputs.Add(new MergeInput(result.FilePath, name, fileSegment.Level));
                    }
                }

                if (segment is ICompositionTitle titleSegment)
                {
                    inputs.Add(new MergeInput(null, titleSegment.SegmentName, titleSegment.Level));
                }
            }

            return EvaluateTitles(inputs);
        }

        private List<IFileEvaluationResult> EvaluateFilesAgainstSegment(ICompositionFile fileSegment, string[] filePaths)
        {
            List<IFileEvaluationResult> found = new List<IFileEvaluationResult>();
            foreach (string file in filePaths)
            {
                IFileEvaluationResult result = fileSegment.EvaluateFile(file);
                if (result.Outcome == OutcomeType.Match)
                {
                    found.Add(result);
                }
            }
            return found;
        }

        private List<IFileEvaluationResult> EvaluateCounts(ICompositionFile fileSegment, 
            List<IFileEvaluationResult> results)
        {
            if (results.Count < fileSegment.MinCount)
            {
                
            }
            if (fileSegment.MaxCount > 0 && results.Count > fileSegment.MaxCount)
            {
                // HERE BE DIALOG FOR CHOOSING RIGHT FILES
            }
            else
            {
                return results;
            }

            return null;
        }

        private List<IMergeInput> EvaluateTitles(List<IMergeInput> inputs)
        {
            List<IMergeInput> results = new List<IMergeInput>();

            for (int i = 0; i < inputs.Count; i++)
            {
                IMergeInput current = inputs[i];

                if (current.FilePath != null)
                {
                    results.Add(current);
                    continue;
                }
                if (i == inputs.Count - 1)
                {
                    break;
                }

                if (inputs[i + 1].Level > current.Level)
                {
                    results.Add(current);
                }
            }
            
            return results;
        }
    }
}
