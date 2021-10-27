using PDFExtractor.Core.Constants;
using PDFExtractor.Core.Singletons;
using PDFExtractor.Modules.Dialog.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PDFExtractor.Modules.Dialog
{
    public class DialogModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var dialog = containerProvider.Resolve<IDialogAssist>();

            dialog.DialogRegionName = RegionNames.SHELL_DIALOG;
            dialog.Add<DialogMessageView>(SchemeNames.Dialog.MESSAGE);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            
        }
    }
}