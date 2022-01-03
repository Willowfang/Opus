using Opus.Modules.MainSection.Views;
using Prism.Ioc;
using Prism.Modularity;
using Opus.Core.Constants;
using Opus.Services.UI;

namespace Opus.Modules.MainSection
{
    public class MainSectionModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<ThreeRegionsView>(RegionNames.SHELL_MAINSECTION, SchemeNames.SPLIT, SchemeNames.SIGNATURE,
                SchemeNames.MERGE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}