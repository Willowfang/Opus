using Opus.Values;
using Opus.Services.UI;
using Opus.Modules.Options.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace Opus.Modules.Options
{
    public class OptionsModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigation = containerProvider.Resolve<INavigationAssist>();

            navigation.Add<ExtractOptionsView>(RegionNames.MAINSECTION_THREE_OPTIONS, SchemeNames.SPLIT);
            navigation.Add<WorkCopyOptionsView>(RegionNames.MAINSECTION_THREE_OPTIONS, SchemeNames.WORKCOPY);
            navigation.Add<MergeOptionsView>(RegionNames.MAINSECTION_THREE_OPTIONS, SchemeNames.MERGE);
            navigation.Add<ComposeOptionsView>(RegionNames.MAINSECTION_THREE_OPTIONS, SchemeNames.COMPOSE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        { }
    }
}