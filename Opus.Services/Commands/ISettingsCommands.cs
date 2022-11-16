using System.Windows.Input;

namespace Opus.Services.Commands
{
    public interface ISettingsCommands
    {
        public ICommand SettingsCommand { get; }
    }

    public interface IComposeSettingsCommands : ISettingsCommands { }

    public interface IExtractSettingsCommands : ISettingsCommands { }

    public interface IMergeSettingsCommands : ISettingsCommands { }

    public interface IWorkCopySettingsCommands : ISettingsCommands { }
}
