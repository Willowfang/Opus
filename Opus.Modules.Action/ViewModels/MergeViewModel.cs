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
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// A viewModel for actions for merging documents.
    /// </summary>
    public class MergeViewModel : ViewModelBaseLogging<MergeViewModel>, INavigationTarget
    {
        #region DI services
        private IEventAggregator eventAggregator;
        private IPathSelection input;
        private IConfiguration configuration;
        private IDialogAssist dialogAssist;
        private IMergingService mergingService;
        #endregion

        #region Fields and properties
        /// <summary>
        /// Collection of files to merge. May be reordered and the level (indentation) of individual file
        /// may be changed.
        /// </summary>
        public ReorderCollection<FileStorage> Collection { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new Merge View Model.
        /// </summary>
        /// <param name="eventAggregator">Service for publishing and receiving events between viewModels.</param>
        /// <param name="input">Service for getting paths as user input.</param>
        /// <param name="navRegistry">Navigation registry for viewModels.</param>
        /// <param name="configuration">Program-wide configurations.</param>
        /// <param name="dialogAssist">Service for showing and otherwise handling dialogs.</param>
        /// <param name="mergingService">Service for handling the actual merging.</param>
        /// <param name="logbook">Logging services.</param>
        public MergeViewModel(
            IEventAggregator eventAggregator,
            IPathSelection input,
            INavigationTargetRegistry navRegistry,
            IConfiguration configuration,
            IDialogAssist dialogAssist,
            IMergingService mergingService,
            ILogbook logbook
        ) : base(logbook)
        {
            // Initialize collection
            Collection = new ReorderCollection<FileStorage>() { CanReorder = true };

            // Assign DI services
            this.eventAggregator = eventAggregator;
            this.input = input;
            this.configuration = configuration;
            this.dialogAssist = dialogAssist;
            this.mergingService = mergingService;

            // Register this viewModel in the navigation registry with the correct scheme.
            navRegistry.AddTarget(SchemeNames.MERGE, this);
        }
        #endregion

        #region INavigationTarget implementation
        /// <summary>
        /// When the view associated with this view model is active and showing, subscribe to receive
        /// notifications, when user selects new files for merging. When inactive and not showing,
        /// do not receive notifications. This token is stored to unsubscribe from the correct event.
        /// </summary>
        SubscriptionToken filesAddedSubscription;

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>. Subscribe to events.
        /// </summary>
        public void OnArrival()
        {
            // Subscribe to file addition event.

            filesAddedSubscription = eventAggregator
                .GetEvent<FilesAddedEvent>()
                .Subscribe(FilesAdded);

            logbook.Write($"{this} subscribed to {nameof(FilesAddedEvent)}.", LogLevel.Debug);
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>. Unsubscribe from events.
        /// </summary>
        public void WhenLeaving()
        {
            // Unsubscribe from file addition event.

            eventAggregator.GetEvent<FilesAddedEvent>().Unsubscribe(filesAddedSubscription);

            logbook.Write($"{this} unsubscribed from {nameof(FilesAddedEvent)}.", LogLevel.Debug);
        }

        /// <summary>
        /// Implementing <see cref="INavigationTarget"/>.
        /// <para>
        /// When reset button is pressed, clear the <see cref="Collection"/>.
        /// </para>
        /// </summary>
        public void Reset()
        {
            Collection?.Clear();
        }

        /// <summary>
        /// When files are selected, add the to the collection.
        /// </summary>
        /// <param name="files">Paths of the files to add.</param>
        private void FilesAdded(string[] files)
        {
            foreach (var file in files)
            {
                Collection.Add(new FileStorage(file));
            }
        }
        #endregion

        #region Commands
        private IAsyncCommand editCommand;

        /// <summary>
        /// Command for editing a file entry.
        /// </summary>
        public IAsyncCommand EditCommand => editCommand ??= new AsyncCommand(ExecuteEditCommand);

        /// <summary>
        /// Execution method for edit command, see <see cref="EditCommand"/>.
        /// <para>
        /// Open a dialog for editing the entry.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        protected async Task ExecuteEditCommand()
        {
            // Create and show a dialog for editing the entry.

            FileTitleDialog fileDialog = new FileTitleDialog(
                Resources.Labels.Dialogs.FileTitle.Edit
            )
            {
                Title = Collection.SelectedItem.Title
            };
            await dialogAssist.Show(fileDialog);

            // If cancelled, just return.

            if (fileDialog.IsCanceled)
            {
                logbook.Write(
                    $"Cancellation requested at {nameof(IDialog)} '{fileDialog}'.",
                    LogLevel.Information
                );
                return;
            }

            // Change the title of the selected item according to user preferences.

            Collection.SelectedItem.Title = fileDialog.Title;
        }

        private DelegateCommand deleteCommand;

        /// <summary>
        /// Command for deleting file entries from <see cref="Collection"/>.
        /// </summary>
        public DelegateCommand DeleteCommand =>
            deleteCommand ??= new DelegateCommand(ExecuteDeleteCommand);

        /// <summary>
        /// Execution method for deletion command, see <see cref="DeleteCommand"/>.
        /// <para>
        /// Delete an entry from the collection.
        /// </para>
        /// </summary>
        protected void ExecuteDeleteCommand()
        {
            Collection.RemoveAll(x => x.IsSelected);
        }

        private DelegateCommand clearCommand;

        /// <summary>
        /// Command for clearing the whole collection.
        /// </summary>
        public DelegateCommand ClearCommand =>
            clearCommand ??= new DelegateCommand(ExecuteClearCommand);

        /// <summary>
        /// Execution method for clear command, see <see cref="ClearCommand"/>.
        /// <para>
        /// Clear the whole collection of all entries.
        /// </para>
        /// </summary>
        protected void ExecuteClearCommand()
        {
            Collection.Clear();
        }

        private IAsyncCommand mergeCommand;

        /// <summary>
        /// Command for executing merge.
        /// </summary>
        public IAsyncCommand MergeCommand => mergeCommand ??= new AsyncCommand(ExecuteMergeCommand);

        /// <summary>
        /// Execution method for merging files command, see <see cref="MergeCommand"/>.
        /// <para>
        /// Merges the files and optionally marks them with page numbers.
        /// </para>
        /// </summary>
        /// <returns>An awaitable task.</returns>
        protected async Task ExecuteMergeCommand()
        {
            string path = input.SaveFile(
                Resources.UserInput.Descriptions.SelectSaveFile,
                FileType.PDF
            );
            if (path == null)
                return;

            IList<IMergeInput> inputs = await Task.Run(GetMergeInputs);

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            ProgressContainer container = dialogAssist.ShowProgress(tokenSource);

            logbook.Write($"Started merging.", LogLevel.Information);

            MergingOptions options = new MergingOptions(
                inputs,
                new FileInfo(path),
                configuration.MergeAddPageNumbers,
                false
            );
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

                MessageDialog message = new MessageDialog(
                    Resources.Labels.General.Error,
                    Resources.Messages.Merging.MergeFailed
                );

                await dialogAssist.Show(message);
            }

            await container.Show;

            logbook.Write($"Merging finished.", LogLevel.Information);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Convert file entries in the <see cref="Collection"/> into
        /// mergeInputs for merging.
        /// </summary>
        /// <returns>List of all inputs for merging.</returns>
        protected IList<IMergeInput> GetMergeInputs()
        {
            List<IMergeInput> inputs = new List<IMergeInput>();
            foreach (FileStorage file in Collection)
            {
                inputs.Add(new MergeInput(file.FilePath, file.Title, file.Level));
            }
            return inputs;
        }
        #endregion
    }
}
