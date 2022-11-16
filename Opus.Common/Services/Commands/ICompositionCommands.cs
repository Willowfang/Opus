using Opus.Common.Services.Commands.Base;
using System.Windows.Input;

namespace Opus.Common.Services.Commands
{
    /// <summary>
    /// Commands for composition action.
    /// </summary>
    public interface ICompositionCommands : IActionCommands
    {
        /// <summary>
        /// Command to change the editability of a profile.
        /// </summary>
        public ICommand EditableCommand { get; }

        /// <summary>
        /// Command for opening the segment menu.
        /// <para>
        /// The relevant boolean is reversed.
        /// </para>
        /// </summary>
        public ICommand OpenSegmentMenuCommand { get; }

        /// <summary>
        /// Command for opening the profile menu.
        /// <para>
        /// The relevant bool is reversed.
        /// </para>
        /// </summary>
        public ICommand OpenProfileMenuCommand { get; }

        /// <summary>
        /// Command for editing the current profile.
        /// </summary>
        public ICommand EditProfileCommand { get; }

        /// <summary>
        /// Command for adding a new profile.
        /// </summary>
        public ICommand AddProfileCommand { get; }

        /// <summary>
        /// Command for deleting the selected profile.
        /// </summary>
        public ICommand DeleteProfileCommand { get; }

        /// <summary>
        /// Command for copying the current profile.
        /// </summary>
        public ICommand CopyProfileCommand { get; }

        /// <summary>
        /// Command for importing a profile from file.
        /// </summary>
        public ICommand ImportProfileCommand { get; }

        /// <summary>
        /// A command to export the selected profile.
        /// </summary>
        public ICommand ExportProfileCommand { get; }

        /// <summary>
        /// Command for adding a new file segment for the profile.
        /// </summary>
        public ICommand AddFileSegmentCommand { get; }

        /// <summary>
        /// Command for editing a profile segment.
        /// </summary>
        public ICommand EditSegmentCommand { get; }

        /// <summary>
        /// Command for adding a title segment (vs. file segment).
        /// </summary>
        public ICommand AddTitleSegmentCommand { get; }

        /// <summary>
        /// Command for deleting a segment from a profile.
        /// </summary>
        public ICommand DeleteSegmentCommand { get; }
    }
}
