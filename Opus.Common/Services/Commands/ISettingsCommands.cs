using System.Windows.Input;

namespace Opus.Common.Services.Commands
{
    /// <summary>
    /// Commands for settings.
    /// </summary>
    public interface ISettingsCommands
    {
        /// <summary>
        /// The command for retrieving settings.
        /// </summary>
        public ICommand SettingsCommand { get; }
    }

    /// <summary>
    /// Commands for composition.
    /// </summary>
    public interface IComposeSettingsCommands : ISettingsCommands { }

    /// <summary>
    /// Commands for extraction.
    /// </summary>
    public interface IExtractSettingsCommands : ISettingsCommands { }

    /// <summary>
    /// Commands for merging.
    /// </summary>
    public interface IMergeSettingsCommands : ISettingsCommands { }

    /// <summary>
    /// Command for work copies.
    /// </summary>
    public interface IWorkCopySettingsCommands : ISettingsCommands { }
}
