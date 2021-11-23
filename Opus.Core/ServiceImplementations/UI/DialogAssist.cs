using Opus.Core.Constants;
using Opus.Services.UI;
using Prism.Events;
using Prism.Regions;

namespace Opus.Core.ServiceImplementations.UI
{
    public class DialogAssist : ViewAssistBase, IDialogAssist
    {
        public string DialogRegionName { get; set; }

        public DialogAssist(IEventAggregator aggregator, IRegionManager manager)
            : base(aggregator, manager) 
        {

        }

        public void Add<T>(string schemeName)
        {
            RegionManager.RegisterViewWithRegion(DialogRegionName, typeof(T));
            Schemes.Add(new SchemeNavigator(schemeName, DialogRegionName, typeof(T).Name));
        }

        protected override void ChangeViews(string schemeName)
        {
            if (schemeName == CurrentScheme)
                return;

            var IRegionCollection = RegionManager.Regions;

            SchemeNavigator navigate = Schemes.Find(x => x.SchemeName == schemeName);
            if (navigate != null)
                RegionManager.RequestNavigate(navigate.RegionName, navigate.ViewName);
        }
    }
}
