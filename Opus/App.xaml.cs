﻿using System.Windows;
using Prism.Ioc;
using Prism.Unity;
using Prism.Modularity;
using Opus.Views;
using System.Globalization;
using System.Threading;
using Opus.ContextMenu;
using Opus.Services.UI;
using Opus.Services.Implementation.UI;
using Opus.Services.Data;
using Opus.Services.Implementation.Data;
using Opus.Core.Constants;
using Opus.Services.Configuration;
using Opus.Services.Implementation.Configuration;
using CX.PdfLib.Services;
using CX.PdfLib.iText7;
using Opus.Services.Input;
using Opus.Services.Implementation.Input;
using System.IO;
using Opus.Services.Implementation.UI.Dialogs;
using Opus.Services.Data.Composition;
using Opus.Services.Implementation.Data.Composition;

namespace Opus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private IContainerProvider initialContainer;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Create a temporary container and register types.
            // Temporary container is used in selecting language
            // and performing I/O operations without initializing UI.
            var extension = CreateContainerExtension();
            initialContainer = extension;
            RegisterTypes(extension);
            SetLanguage(initialContainer);
            UpdateProfiles(initialContainer);

            // If started with arguments, display no UI,
            // run command and exit.
            if (e.Args.Length > 0)
            {
                StartUpNoUI(extension, e.Args);
                Current.Shutdown();
            }
            // Otherwise, initialize Prism and show UI
            else
            {
                initialContainer = null;
                base.OnStartup(e);
            }
        }

        /// <summary>
        /// Perform the command line command provided in arguments
        /// </summary>
        /// <param name="container">Temporary container for registered types</param>
        /// <param name="arguments">Given arguments</param>
        private void StartUpNoUI(IContainerExtension container, string[] arguments)
        {
            if (arguments[0] == "-remove") container.Register<IContextMenuCommand, RemoveSignature>();
            if (arguments[0] == "-split") container.Register<IContextMenuCommand, ExtractDocument>();
            if (arguments[0] == "-splitdir") container.Register<IContextMenuCommand, ExtractDirectory>();

            if (container.IsRegistered(typeof(IContextMenuCommand)))
                container.Resolve<IContextMenuCommand>().RunCommand(arguments);
        }

        /// <summary>
        /// Set application display language
        /// </summary>
        /// <param name="container">Temporary container for registered types</param>
        private void SetLanguage(IContainerProvider container)
        {
            CultureInfo ci = new CultureInfo(container.Resolve<IConfiguration>().LanguageCode);
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        private void UpdateProfiles(IContainerProvider container)
        {
            ICompositionOptions options = container.Resolve<ICompositionOptions>();

            bool errorFlag = false;

            if (Directory.Exists(FilePaths.PROFILE_DIRECTORY) == false) return;

            foreach (string filePath in Directory.GetFiles(FilePaths.PROFILE_DIRECTORY))
            {
                try
                {
                    ICompositionProfile profile = options.ImportProfile(filePath);
                    options.SaveProfile(profile);
                    File.Delete(filePath);
                }
                catch
                {
                    errorFlag = true;
                }
            }

            if (errorFlag)
            {
                MessageBox.Show(Opus.Resources.Messages.StartUp.ProfileUpdateFailed,
                        Opus.Resources.Labels.General.Error);
            }
        }

        // Overrides for Prism application methods
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Configuration services
            string configPath = Path.Combine(FilePaths.CONFIG_DIRECTORY, "Config" + FilePaths.CONFIG_EXTENSION);
            containerRegistry.RegisterSingleton<IConfiguration>(x => Configuration.Load(configPath));

            // Services for manipulating data
            containerRegistry.Register<IBookmarker, Bookmarker>();
            containerRegistry.Register<IExtractor, Extractor>();
            containerRegistry.Register<ISigner, Signer>();
            containerRegistry.Register<IMerger, Merger>();
            containerRegistry.Register<IConverter, ConverterWord>();
            containerRegistry.Register<IManipulator, Manipulator>();

            // UI-related services
            containerRegistry.RegisterManySingleton<NavigationAssist>(
                typeof(INavigationAssist),
                typeof(INavigationTargetRegistry));
            containerRegistry.Register<IPathSelection, PathSelectionWin>();
            containerRegistry.RegisterSingleton<IDialogAssist, DialogAssist>();

            // Data services
            var provider = new DataProviderLiteDB(Path.Combine(FilePaths.CONFIG_DIRECTORY,
                "App" + FilePaths.CONFIG_EXTENSION));
            containerRegistry.RegisterInstance<IDataProvider>(provider);
            containerRegistry.RegisterSingleton<ISignatureOptions, SignatureOptions>();
            containerRegistry.RegisterSingleton<ICompositionOptions, CompositionOptions>();
            containerRegistry.Register<IComposerFactory, ComposerFactory>();
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Modules.MainSection.MainSectionModule>();
            moduleCatalog.AddModule<Modules.File.FileModule>();
            moduleCatalog.AddModule<Modules.Action.ActionModule>();
            moduleCatalog.AddModule<Modules.Options.OptionsModule>();
        }
        protected override Window CreateShell()
        {
            var w = Container.Resolve<MainWindowView>();
            return w;
        }
    }
}
