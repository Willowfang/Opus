using Opus.Values;
using Opus.Services.UI;
using Opus.Modules.Options.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace Opus.Modules.Options
{
    /// <summary>
    /// Register module for options region.
    /// </summary>
    public class OptionsModule : IModule
    {
        /// <summary>
        /// Assign correct schemes to correct views.
        /// </summary>
        /// <param name="containerProvider">Container provider</param>
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var navigation = containerProvider.Resolve<INavigationAssist>();

            navigation.Add<ExtractOptionsView>(
                RegionNames.MAINSECTION_FOUR_OPTIONS,
                SchemeNames.EXTRACT
            );
            navigation.Add<WorkCopyOptionsView>(
                RegionNames.MAINSECTION_FOUR_OPTIONS,
                SchemeNames.WORKCOPY
            );
            navigation.Add<MergeOptionsView>(
                RegionNames.MAINSECTION_FOUR_OPTIONS,
                SchemeNames.MERGE
            );
            navigation.Add<ComposeOptionsView>(
                RegionNames.MAINSECTION_FOUR_OPTIONS,
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
