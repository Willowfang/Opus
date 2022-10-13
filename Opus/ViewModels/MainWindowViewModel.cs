using System;
using System.Diagnostics;
using System.IO;
using Opus.Core.Base;
using Opus.Values;
using Opus.Services.Configuration;
using Prism.Commands;
using Prism.Events;
using Opus.Services.UI;
using Opus.Events;
using Opus.Services.Implementation.UI.Dialogs;
using AsyncAwaitBestPractices.MVVM;
using System.Threading.Tasks;
using System.Windows;
using CX.LoggingLib;
using System.Collections;
using Opus.Core.Wrappers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Opus.Core.Executors;

namespace Opus.ViewModels
{
    /// <summary>
    /// Responsible for handling UX when the program is started
    /// in full GUI mode
    /// </summary>
    public class MainWindowViewModel : ViewModelBaseLogging<MainWindowViewModel>
    {
        private readonly IConfiguration configuration;
        private readonly IEventAggregator eventAggregator;

        private string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        /// <summary>
        /// Responsible for showing, closing and otherwise
        /// handling all dialogs.
        /// </summary>
        public IDialogAssist Dialog { get; set; }

        public ObservableCollection<LanguageOption> Languages { get; }

        public MainWindowViewModel(
            IEventAggregator eventAggregator, 
            IConfiguration config, 
            IDialogAssist dialogAssist,
            IUpdateExecutor updateExecutor,
            ILogbook logbook) : base(logbook)
        {
            configuration = config;
            this.eventAggregator = eventAggregator;
            this.Dialog = dialogAssist;
            Languages = new ObservableCollection<LanguageOption>();

            foreach (string code in SupportedTypes.CULTURES)
            {
                Languages.Add(new LanguageOption(code, Resources.Labels.MainWindow.Languages.ResourceManager.GetString(code)));
            }
        }

        #region LICENSE
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
        #endregion LICENSE

        #region MANUAL
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
        #endregion MANUAL

        #region SOURCE CODE
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
        #endregion SOURCE CODE

        #region LANGUAGE
        private IAsyncCommand<string> _languageCommand;
        public IAsyncCommand<string> LanguageCommand =>
            _languageCommand ??= new AsyncCommand<string>(ExecuteLanguage);
        private async Task ExecuteLanguage(string language)
        {
            logbook.Write($"Change of language to {language} requested.", LogLevel.Information);

            var lang = configuration.LanguageCode;
            if (language == lang)
                return;

            configuration.LanguageCode = language;
            await Dialog.Show(new MessageDialog(Resources.Labels.General.Notification,
                Resources.Messages.MainWindow.ChangeLanguage));
        }
        #endregion LANGUAGE

        #region NAVIGATION
        private DelegateCommand<string> _navigateCommand;
        public DelegateCommand<string> NavigateCommand =>
            _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(ExecuteNavigation));
        void ExecuteNavigation(string name)
        {
            logbook.Write($"Navigation requested for scheme {name}.", LogLevel.Information);

            eventAggregator.GetEvent<ViewChangeEvent>().Publish(name);
            if (name == SchemeNames.SPLIT)
                Title = Resources.Labels.MainWindow.Titles.Extract.ToUpper();
            if (name == SchemeNames.WORKCOPY)
                Title = Resources.Labels.MainWindow.Titles.Workcopy.ToUpper();
            if (name == SchemeNames.MERGE)
                Title = Resources.Labels.MainWindow.Titles.Merge.ToUpper();
            if (name == SchemeNames.COMPOSE)
                Title = Resources.Labels.MainWindow.Titles.Compose.ToUpper();
        }

        private DelegateCommand exitCommand;
        public DelegateCommand ExitCommand =>
            exitCommand ??= new DelegateCommand(ExecuteExit);
        private void ExecuteExit()
        {
            Application.Current.Shutdown();
        }
        #endregion NAVIGATION

        #region OTHERS
        private DelegateCommand resetCommand;
        public DelegateCommand ResetCommand =>
            resetCommand ?? (resetCommand = new DelegateCommand(ExecuteReset));

        private void ExecuteReset()
        {
            eventAggregator.GetEvent<ActionResetEvent>().Publish();
        }
        #endregion
    }
}
