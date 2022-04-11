using AsyncAwaitBestPractices.MVVM;
using CX.LoggingLib;
using CX.PdfLib.Common;
using CX.PdfLib.Services;
using CX.PdfLib.Services.Data;
using Opus.Core.Base;
using Opus.Values;
using Opus.Core.Wrappers;
using Opus.Events;
using Opus.Services.Configuration;
using Opus.Services.Extensions;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Input;
using Opus.Services.UI;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Opus.Modules.Action.ViewModels
{
    public class MergeViewModel : ViewModelBaseLogging<MergeViewModel>, INavigationTarget
    {
        /// <summary>
        /// Common event handling service
        /// </summary>
        private IEventAggregator eventAggregator;
        /// <summary>
        /// Service for performing manipulations on a pdf-file
        /// </summary>
        /// <summary>
        /// Service handling user input for file and directory paths
        /// </summary>
        private IPathSelection input;
        /// <summary>
        /// Merge-specific configuration
        /// </summary>
        private IConfiguration configuration;
        /// <summary>
        /// Common service for displaying and updating a dialog
        /// </summary>
        private IDialogAssist dialogAssist;
        private IMergingService mergingService;

        /// <summary>
        /// Files to be merged
        /// </summary>
        public ReorderCollection<FileStorage> Collection { get; }

        public MergeViewModel(
            IEventAggregator eventAggregator,
            IPathSelection input, 
            INavigationTargetRegistry navRegistry, 
            IConfiguration configuration, 
            IDialogAssist dialogAssist,
            IMergingService mergingService,
            ILogbook logbook) : base(logbook)
        {
            Collection = new ReorderCollection<FileStorage>()
            {
                CanReorder = true
            };
            this.eventAggregator = eventAggregator;
            this.input = input;
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.mergingService = mergingService;
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

            logbook.Write($"{this} subscribed to {nameof(FilesAddedEvent)}.", LogLevel.Debug);
        }
        public void WhenLeaving()
        {
            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);

            logbook.Write($"{this} unsubscribed from {nameof(FilesAddedEvent)}.", LogLevel.Debug);
        }
        private void FilesAdded(string[] files)
        {
            foreach (var file in files)
            {
                Collection.Add(new FileStorage(file));
            }
        }

        private IAsyncCommand editCommand;
        public IAsyncCommand EditCommand => editCommand ??=
            new AsyncCommand(ExecuteEditCommand);
        private async Task ExecuteEditCommand()
        {
            FileTitleDialog fileDialog = new FileTitleDialog(Resources.Labels.Dialogs.FileTitle.Edit)
            {
                Title = Collection.SelectedItem.Title
            };
            await dialogAssist.Show(fileDialog);

            if (fileDialog.IsCanceled)
            {
                logbook.Write($"Cancellation requested at {nameof(IDialog)} '{fileDialog}'.", LogLevel.Information);
                return;
            }

            Collection.SelectedItem.Title = fileDialog.Title;
        }

        // Delete selected selected containers from collection
        private DelegateCommand deleteCommand;
        public DelegateCommand DeleteCommand => deleteCommand ??= 
            new DelegateCommand(ExecuteDeleteCommand);
        private void ExecuteDeleteCommand()
        {
            Collection.RemoveAll(x => x.IsSelected);
        }

        // Delete all containers from collection
        private DelegateCommand clearCommand;
        public DelegateCommand ClearCommand => clearCommand ??= 
            new DelegateCommand(ExecuteClearCommand);
        private void ExecuteClearCommand()
        {
            Collection.Clear();
        }

        // Merge files. Get option selection for stamping the pages with page numbers.
        private IAsyncCommand mergeCommand;
        public IAsyncCommand MergeCommand => mergeCommand ??=
            new AsyncCommand(ExecuteMergeCommand);
        private async Task ExecuteMergeCommand()
        {
            string path = input.SaveFile(Resources.UserInput.Descriptions.SelectSaveFile, FileType.PDF);
            if (path == null) return;

            IList<IMergeInput> inputs = await Task.Run(GetMergeInputs);

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            ProgressContainer container = dialogAssist.ShowProgress(tokenSource);

            logbook.Write($"Started merging.", LogLevel.Information);

            MergingOptions options = new MergingOptions(inputs, new FileInfo(path), configuration.MergeAddPageNumbers, false);
            options.Cancellation = token;
            options.Progress = container.Reporting;

            try
            {
                await mergingService.MergeWithOptions(options);
            }
            catch (Exception e)
            {
                logbook.Write($"Merging encountered an error", LogLevel.Error, e);

                container.ProgressDialog.CloseOnError();

                MessageDialog message = new MessageDialog(Resources.Labels.General.Error,
                    Resources.Messages.Merging.MergeFailed);

                await dialogAssist.Show(message);
            }

            await container.Show;

            logbook.Write($"Merging finished.", LogLevel.Information);
        }

        private IList<IMergeInput> GetMergeInputs()
        {
            List<IMergeInput> inputs = new List<IMergeInput>();
            foreach (FileStorage file in Collection)
            {
                inputs.Add(new MergeInput(file.FilePath, file.Title, file.Level));
            }
            return inputs;
        }
    }
}
