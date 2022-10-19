using Opus.Modules.MainSection.Views;
using Prism.Ioc;
using Prism.Modularity;
using Opus.Values;
using Opus.Services.UI;

namespace Opus.Modules.MainSection
{
    /// <summary>
    /// A module for displaying different regions in the main window.
    /// </summary>
    public class MainSectionModule : IModule
    {
        /// <summary>
        /// Register correct reginos with correct schemes.
        /// </summary>
        /// <param name="containerProvider">Container provider.</param>
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<FourRegionsView>(RegionNames.SHELL_MAINSECTION, SchemeNames.EXTRACT);
            navigator.Add<ThreeRegionsView>(
                RegionNames.SHELL_MAINSECTION,
                SchemeNames.WORKCOPY,
                SchemeNames.MERGE,
                SchemeNames.COMPOSE
            );
        }

        /// <summary>
        /// No types to register here.
        /// </summary>
        /// <param name="containerRegistry">Container registry.</param>
        public void RegisterTypes(IContainerRegistry containerRegistry) { }
    }
}
