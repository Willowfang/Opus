using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Opus.Modules.Action.Views;
using Opus.Values;
using Opus.Services.UI;
using Opus.Core.Wrappers;

namespace Opus.Modules.Action
{
    public class ActionModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigator = containerProvider.Resolve<INavigationAssist>();

            navigator.Add<ExtractionView>(RegionNames.MAINSECTION_FOUR_ACTION, SchemeNames.EXTRACT);
            navigator.Add<ExtractionOrderView>(
                RegionNames.MAINSECTION_FOUR_SUPPORT,
                SchemeNames.EXTRACT
            );
            navigator.Add<WorkCopyView>(RegionNames.MAINSECTION_THREE_ACTION, SchemeNames.WORKCOPY);
            navigator.Add<MergeView>(RegionNames.MAINSECTION_THREE_ACTION, SchemeNames.MERGE);
            navigator.Add<CompositionView>(
                RegionNames.MAINSECTION_THREE_ACTION,
                SchemeNames.COMPOSE
            );
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }
    }
}
