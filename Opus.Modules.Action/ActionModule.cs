using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Opus.Modules.Action.Views;
using Opus.Core.Constants;
using Opus.Services.UI;
using Opus.Core.Wrappers;
using Opus.Services.Implementation.UI;

namespace Opus.Modules.Action
{
    public class ActionModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<BookmarksView>(RegionNames.MAINSECTION_THREE_ACTION, SchemeNames.SPLIT);
            navigator.Add<SignatureRemovalView>(RegionNames.MAINSECTION_THREE_ACTION, SchemeNames.SIGNATURE);
            navigator.Add<MergeView>(RegionNames.MAINSECTION_THREE_ACTION, SchemeNames.MERGE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IReorderCollection<FileStorage>, ReorderCollection<FileStorage>>();
        }
    }
}