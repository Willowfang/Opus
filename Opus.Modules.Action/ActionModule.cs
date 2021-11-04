using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Opus.Modules.Action.Views;
using Opus.Core.Constants;
using Opus.Services.UI;

namespace Opus.Modules.Action
{
    public class ActionModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<BookmarksView>(RegionNames.MAINSECTION_THREE_ACTION, SchemeNames.SPLIT);
            navigator.Add<SignatureRemovalView>(RegionNames.MAINSECTION_THREE_ACTION, SchemeNames.SIGNATURE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}