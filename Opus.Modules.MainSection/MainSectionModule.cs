using Opus.Modules.MainSection.Views;
using Prism.Ioc;
using Prism.Modularity;
using Opus.Values;
using Opus.Services.UI;

namespace Opus.Modules.MainSection
{
    public class MainSectionModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<ThreeRegionsView>(RegionNames.SHELL_MAINSECTION, SchemeNames.SPLIT, SchemeNames.WORKCOPY,
                SchemeNames.MERGE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}