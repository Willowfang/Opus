using CX.PdfLib.Common;
using CX.PdfLib.Services.Data;
using Opus.Core.Base;
using Opus.Core.Constants;
using Opus.Events;
using Opus.Services.Configuration;
using Opus.Services.Data;
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
using System.Threading.Tasks;
using System.Windows.Input;

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
        private IConfiguration.Compose configuration;
        /// <summary>
        /// Common service for dialog control
        /// </summary>
        private IDialogAssist dialogAssist;
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
        private IReorderCollection<ICompositionSegment> segments;
        public IReorderCollection<ICompositionSegment> Segments
        {
            get => segments;
            set => SetProperty(ref segments, value);
        }

        /// <summary>
        /// The profile that is currently selected
        /// </summary>
        private ICompositionProfile selectedProfile;
        public ICompositionProfile SelectedProfile
        {
            get => selectedProfile;
            set => SetProperty(ref selectedProfile, value);
        }
        /// <summary>
        /// Currently selected segment from <see cref="SelectedProfile"/>
        /// </summary>
        private ICompositionSegment selectedSegment;
        public ICompositionSegment SelectedSegment
        {
            get => selectedSegment;
            set => SetProperty(ref selectedSegment, value);
        }

        private bool isEditEnabled;
        public bool IsEditEnabled
        {
            get => isEditEnabled;
            set => SetProperty(ref isEditEnabled, value);
        }

        public CompositionViewModel(IEventAggregator aggregator, IManipulator manipulator,
            IPathSelection input, IConfiguration.Compose configuration, INavigationTargetRegistry navReg,
            IDialogAssist dialogAssist)
        {
            this.aggregator = aggregator;
            this.manipulator = manipulator;
            this.input = input;
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;

            Profiles = new ObservableCollection<ICompositionProfile>(configuration.GetProfiles());
            navReg.AddTarget(SchemeNames.COMPOSE, this);
        }

        /// <summary>
        /// When the view associated with this view model is active and showing, subscribe to receive
        /// notifications of enabling edit mode, new profiles, new segments and selected directory. 
        /// When inactive and not showing, do not receive notifications.
        /// </summary>
        SubscriptionToken editEnabledSubscription;
        SubscriptionToken profileAddedSubscription;
        SubscriptionToken segmentAddedSubscription;
        SubscriptionToken directorySelectedSubscription;
        public void OnArrival()
        {
            editEnabledSubscription = aggregator.GetEvent<EditEnableEvent>().Subscribe(x => IsEditEnabled = x);
            profileAddedSubscription =
                aggregator.GetEvent<CompositionProfileEvent>().Subscribe(AddProfile);
            segmentAddedSubscription =
                aggregator.GetEvent<CompositionSegmentEvent>().Subscribe(AddSegment);
        }
        public void WhenLeaving()
        {
            aggregator.GetEvent<EditEnableEvent>().Unsubscribe(editEnabledSubscription);
            aggregator.GetEvent<CompositionProfileEvent>().Unsubscribe(profileAddedSubscription);
            aggregator.GetEvent<CompositionSegmentEvent>().Unsubscribe(segmentAddedSubscription);
        }

        private void AddProfile(ICompositionProfile profile)
        {
            Profiles.Add(profile);
            SelectedProfile = profile;
        }
        private void AddSegment(ICompositionSegment segment)
        {
            SelectedProfile.Segments.Items.Add(segment);
            SelectedSegment = segment;
        }

        private async Task Compose()
        {
            if (selectedDirectory == null)
            {
                dialogAssist.Show(new MessageDialog(Resources.DialogMessages.Composition_DirectoryNull));
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

            foreach (ICompositionSegment segment in SelectedProfile.Segments.Items)
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
