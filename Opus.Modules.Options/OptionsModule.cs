using Opus.Core.Constants;
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

            navigation.Add<BookmarkNewView>(RegionNames.MAINSECTION_THREE_OPTIONS, SchemeNames.SPLIT);
            navigation.Add<IdentifierView>(RegionNames.MAINSECTION_THREE_OPTIONS, SchemeNames.SIGNATURE);
            navigation.Add<MergeOptionsView>(RegionNames.MAINSECTION_THREE_OPTIONS, SchemeNames.MERGE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        { }
    }
}