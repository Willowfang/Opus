using AsyncAwaitBestPractices.MVVM;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using Opus.Core.Base;
using Opus.Core.Constants;
using Opus.Core.ExtensionMethods;
using Opus.Core.Wrappers;
using Opus.Events;
using Opus.Services.Configuration;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Input;
using Opus.Services.UI;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Opus.Modules.Action.ViewModels
{
    public class MergeViewModel : ViewModelBase, INavigationTarget
    {
        /// <summary>
        /// Common event handling service
        /// </summary>
        private IEventAggregator eventAggregator;
        /// <summary>
        /// Service for performing manipulations on a pdf-file
        /// </summary>
        private IManipulator manipulator;
        /// <summary>
        /// Service handling user input for file and directory paths
        /// </summary>
        private IPathSelection input;
        /// <summary>
        /// Merge-specific configuration
        /// </summary>
        private IConfiguration.Merge configuration;
        /// <summary>
        /// Common service for displaying and updating a dialog
        /// </summary>
        private IDialogAssist dialogAssist;

        /// <summary>
        /// Files to be merged
        /// </summary>
        public IReorderCollection<FileStorage> Collection { get; }
        /// <summary>
        /// Currently selected file
        /// </summary>
        private FileStorage selectedFile;
        public FileStorage SelectedFile
        {
            get => selectedFile;
            set => SetProperty(ref selectedFile, value);
        }

        public MergeViewModel(IEventAggregator eventAggregator, IManipulator manipulator,
            IPathSelection input, INavigationTargetRegistry navRegistry,
            IConfiguration.Merge configuration, IReorderCollection<FileStorage> collection,
            IDialogAssist dialogAssist)
        {
            Collection = collection;
            this.eventAggregator = eventAggregator;
            this.manipulator = manipulator;
            this.input = input;
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            navRegistry.AddTarget(SchemeNames.MERGE, this);
        }

        /// <summary>
        /// When the view associated with this view model is active and showing, subscribe to receive
        /// notifications, when user selects new files for merging. When inactive and not showing,
        /// do not receive notifications.
        /// </summary>
        SubscriptionToken filesAddedSubscription;
        public void OnArrival()
        {
            filesAddedSubscription = eventAggregator.GetEvent<FilesAddedEvent>().Subscribe(FilesAdded);
        }
        public void WhenLeaving()
        {
            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);
        }
        private void FilesAdded(string[] files)
        {
            foreach (var file in files)
            {
                Collection.Items.Add(new FileStorage(file));
            }
        }

        // Delete selected selected containers from collection
        private DelegateCommand deleteCommand;
        public DelegateCommand DeleteCommand => deleteCommand ??= 
            new DelegateCommand(ExecuteDeleteCommand);
        private void ExecuteDeleteCommand()
        {
            Collection.Items.RemoveAll(x => x.IsSelected);
        }

        // Delete all containers from collection
        private DelegateCommand clearCommand;
        public DelegateCommand ClearCommand => clearCommand ??= 
            new DelegateCommand(ExecuteClearCommand);
        private void ExecuteClearCommand()
        {
            Collection.Items.Clear();
        }

        // Merge files. Get option selection for stamping the pages with page numbers.
        private IAsyncCommand mergeCommand;
        public IAsyncCommand MergeCommand => mergeCommand ??=
            new AsyncCommand(ExecuteMergeCommand);
        private async Task ExecuteMergeCommand()
        {
            string path = input.SaveFile(Resources.Labels.Bookmarks_SelectPath, FileType.PDF);
            if (path == null) return;

            IList<IMergeInput> inputs = await Task.Run(GetMergeInputs);
            var result = ShowProgress();
            Task merge = manipulator.MergeWithBookmarksAsync(inputs, path, configuration.AddPageNumbers, result.progress);

            await result.dialog;
        }

        private IList<IMergeInput> GetMergeInputs()
        {
            List<IMergeInput> inputs = new List<IMergeInput>();
            foreach (FileStorage file in Collection.Items)
            {
                inputs.Add(new MergeInput(file.FilePath, file.Title, file.Level));
            }
            return inputs;
        }

        private (Task dialog, IProgress<ProgressReport> progress) ShowProgress()
        {
            ProgressDialog dialog = new ProgressDialog()
            {
                TotalPercent = 0,
                Phase = ProgressPhase.Unassigned.GetResourceString()
            };
            Progress<ProgressReport> progress = new Progress<ProgressReport>(report =>
            {
                dialog.TotalPercent = report.Percentage;
                dialog.Phase = report.CurrentPhase.GetResourceString();
                dialog.Part = report.CurrentItem;
            });

            return (dialogAssist.Show(dialog), progress);
        }
    }
}
