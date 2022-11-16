using AsyncAwaitBestPractices.MVVM;
using Opus.Actions.Services.Compose;
using Opus.Common.Services.Commands;
using Prism.Commands;
using System.Windows.Input;

namespace Opus.Commands.Implementation
{
    /// <summary>
    /// Implementation for <see cref="ICompositionCommands"/>
    /// </summary>
    public class CompositionCommands : ICompositionCommands
    {
        private readonly ICompositionProperties properties;
        private readonly ICompositionMethods methods;

        /// <summary>
        /// Create new implementation instance.
        /// </summary>
        /// <param name="properties">Composition properties service.</param>
        /// <param name="methods">Composition methods service.</param>
        public CompositionCommands(
            ICompositionProperties properties,
            ICompositionMethods methods)
        {
            this.properties = properties;
            this.methods = methods;
        }

        private DelegateCommand? editableCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand EditableCommand =>
            editableCommand ??= new DelegateCommand(methods.ExecuteEditable);

        private DelegateCommand? openSegmentMenuCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand OpenSegmentMenuCommand =>
            openSegmentMenuCommand ??= new DelegateCommand(
                () => properties.AddSegmentMenuOpen = !properties.AddSegmentMenuOpen);

        private DelegateCommand? openProfileMenuCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand OpenProfileMenuCommand =>
            openProfileMenuCommand ??= new DelegateCommand(
                () => properties.AddProfileMenuOpen = !properties.AddSegmentMenuOpen);

        private IAsyncCommand? editProfileCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand EditProfileCommand =>
            editProfileCommand ??= new AsyncCommand(methods.ExecuteEditProfile);

        private IAsyncCommand? addProfileCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand AddProfileCommand =>
            addProfileCommand ??= new AsyncCommand(methods.ExecuteAddProfile);

        private IAsyncCommand? deleteProfileCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand DeleteProfileCommand =>
            deleteProfileCommand ??= new AsyncCommand(methods.ExecuteDeleteProfile);

        private IAsyncCommand? copyProfileCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand CopyProfileCommand =>
            copyProfileCommand ??= new AsyncCommand(methods.ExecuteCopyProfile);

        private IAsyncCommand? importProfileCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand ImportProfileCommand =>
            importProfileCommand ??= new AsyncCommand(methods.ExecuteImportProfile);

        private IAsyncCommand? exportProfileCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand ExportProfileCommand =>
            exportProfileCommand ??= new AsyncCommand(methods.ExecuteExportProfile);

        private IAsyncCommand? addFileSegmentCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand AddFileSegmentCommand =>
            addFileSegmentCommand ??= new AsyncCommand(methods.ExecuteAddFileSegment);

        private IAsyncCommand? editSegmentCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand EditSegmentCommand =>
            editSegmentCommand ??= new AsyncCommand(methods.ExecuteEditSegment);

        private IAsyncCommand? addTitleSegmentCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand AddTitleSegmentCommand =>
            addTitleSegmentCommand ??= new AsyncCommand(methods.ExecuteAddTitleSegment);

        private IAsyncCommand? deleteSegmentCommand;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ICommand DeleteSegmentCommand =>
            deleteSegmentCommand ??= new AsyncCommand(methods.ExecuteDeleteSegment);
    }
}
