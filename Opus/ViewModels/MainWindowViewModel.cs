using System;
using System.Diagnostics;
using System.IO;
using Opus.Core.Base;
using Opus.Core.Constants;
using Opus.Services.Configuration;
using Prism.Commands;
using Prism.Events;
using Opus.Services.UI;
using Opus.Events;
using Opus.Services.Implementation.UI.Dialogs;
using AsyncAwaitBestPractices.MVVM;
using System.Threading.Tasks;

namespace Opus.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IConfiguration configuration;
        private IEventAggregator eventAggregator;

        private string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public IDialogAssist Dialog { get; set; }


        public MainWindowViewModel(IEventAggregator eventAggregator, 
            IConfiguration config, IDialogAssist dialogAssist)
        {
            configuration = config;
            this.eventAggregator = eventAggregator;
            this.Dialog = dialogAssist;
        }

        private DelegateCommand openLicenses;
        public DelegateCommand OpenLicenses =>
            openLicenses ?? (openLicenses = new DelegateCommand(ExecuteOpenLicenses));

        void ExecuteOpenLicenses()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "TextFiles", Resources.Hyperlinks.Hyperlinks.Licenses))
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private DelegateCommand openManual;
        public DelegateCommand OpenManual =>
            openManual ?? (openManual = new DelegateCommand(ExecuteOpenManual));

        void ExecuteOpenManual()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@Resources.Hyperlinks.Hyperlinks.UserManual)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private DelegateCommand openSourceCode;
        public DelegateCommand OpenSourceCode =>
            openSourceCode ?? (openSourceCode = new DelegateCommand(ExecuteOpenSourceCode));

        void ExecuteOpenSourceCode()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@Resources.Hyperlinks.Hyperlinks.SourceCode)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private DelegateCommand<string> _navigateCommand;
        public DelegateCommand<string> NavigateCommand =>
            _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(ExecuteNavigation));

        void ExecuteNavigation(string name)
        {
            eventAggregator.GetEvent<ViewChangeEvent>().Publish(name);
            if (name == SchemeNames.SPLIT)
                Title = Resources.Labels.MainWindow.Titles.Extract.ToUpper();
            if (name == SchemeNames.SIGNATURE)
                Title = Resources.Labels.MainWindow.Titles.Signature.ToUpper();
            if (name == SchemeNames.MERGE)
                Title = Resources.Labels.MainWindow.Titles.Merge.ToUpper();
            if (name == SchemeNames.COMPOSE)
                Title = Resources.Labels.MainWindow.Titles.Compose.ToUpper();
        }

        private IAsyncCommand<string> _languageCommand;
        public IAsyncCommand<string> LanguageCommand =>
            _languageCommand ?? (_languageCommand = new AsyncCommand<string>(ExecuteLanguage));

        private async Task ExecuteLanguage(string language)
        {
            var lang = configuration.LanguageCode;
            if (language == lang)
                return;

            configuration.LanguageCode = language;
            await Dialog.Show(new MessageDialog(Resources.Labels.General.Notification,
                Resources.Messages.MainWindow.ChangeLanguage));
        }
    }
}
