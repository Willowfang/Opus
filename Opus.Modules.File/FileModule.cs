using Opus.Modules.File.Views;
using Prism.Ioc;
using Prism.Modularity;
using Opus.Values;
using Opus.Services.UI;

namespace Opus.Modules.File
{
    /// <summary>
    /// Module for file selection region and its views.
    /// </summary>
    public class FileModule : IModule
    {
        /// <summary>
        /// Register correct views for right schemes.
        /// </summary>
        /// <param name="containerProvider">Container provider.</param>
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<FileMultipleView>(RegionNames.MAINSECTION_FOUR_FILE, SchemeNames.EXTRACT);
            navigator.Add<FileMultipleView>(
                RegionNames.MAINSECTION_FOUR_FILE,
                SchemeNames.WORKCOPY
            );
            navigator.Add<FileMultipleView>(RegionNames.MAINSECTION_FOUR_FILE, SchemeNames.MERGE);
            navigator.Add<DirectoryNavigationView>(
                RegionNames.MAINSECTION_FOUR_FILE,
                SchemeNames.COMPOSE
            );
        }

        /// <summary>
        /// No types to register here.
        /// </summary>
        /// <param name="containerRegistry"></param>
        public void RegisterTypes(IContainerRegistry containerRegistry) { }
    }
}
