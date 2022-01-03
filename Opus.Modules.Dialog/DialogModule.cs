using Opus.Core.Constants;
using Opus.Services.UI;
using Opus.Modules.Dialog.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Opus.Modules.Dialog
{
    public class DialogModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var dialog = containerProvider.Resolve<IDialogAssist>();
            var manager = containerProvider.Resolve<IRegionManager>();
            manager.RegisterViewWithRegion<DialogContentView>(RegionNames.SHELL_DIALOG);

            dialog.DialogRegionName = RegionNames.DIALOG_CONTENT;
            dialog.Add<DialogMessageView>(SchemeNames.Dialog.MESSAGE);
            dialog.Add<DialogProgressView>(SchemeNames.Dialog.PROGRESS);

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}