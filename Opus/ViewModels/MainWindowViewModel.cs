using System;
using System.Diagnostics;
using System.IO;
using Opus.Core.Base;
using Opus.Core.Constants;
using Opus.Core.Events;
using Opus.Services.Configuration;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Opus.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private bool dialogIsShowing;
        public bool DialogIsShowing
        {
            get { return dialogIsShowing; }
            set { SetProperty(ref dialogIsShowing, value); }
        }

        private IConfiguration.App Configuration;

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, 
            IConfiguration.App config)
            : base(regionManager, eventAggregator)
        {
            eventAggregator.GetEvent<DialogMessageEvent>().Subscribe(ShowDialogMessage);
            Configuration = config;
        }

        private void ShowDialogMessage(string message)
        {
            DialogIsShowing = true;
        }

        private DelegateCommand openLicenses;
        public DelegateCommand OpenLicenses =>
            openLicenses ?? (openLicenses = new DelegateCommand(ExecuteOpenLicenses));

        void ExecuteOpenLicenses()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "TextFiles", Resources.Hyperlinks.Licenses))
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
            p.StartInfo = new ProcessStartInfo(@Resources.Hyperlinks.UserManual)
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
            p.StartInfo = new ProcessStartInfo(@Resources.Hyperlinks.SourceCode)
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
            Aggregator.GetEvent<ViewChangeEvent>().Publish(name);
            if (name == SchemeNames.SPLIT)
                Title = Resources.Labels.Title_Split.ToUpper();
            if (name == SchemeNames.SIGNATURE)
                Title = Resources.Labels.Title_Signature.ToUpper();
        }

        private DelegateCommand<string> _languageCommand;
        public DelegateCommand<string> LanguageCommand =>
            _languageCommand ?? (_languageCommand = new DelegateCommand<string>(ExecuteLanguage));

        void ExecuteLanguage(string language)
        {
            var lang = Configuration.GetLanguage();
            if (language == lang)
                return;

            Configuration.ChangeLanguage(language);
            Aggregator.GetEvent<DialogMessageEvent>().Publish(Resources.Messages.LanguageChange);
        }
    }
}
