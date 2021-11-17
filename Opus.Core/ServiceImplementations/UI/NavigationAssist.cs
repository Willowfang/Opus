using System.Linq;
using Opus.Services.UI;
using Prism.Events;
using Prism.Regions;

namespace Opus.Core.ServiceImplementations.UI
{
    public class NavigationAssist : ViewAssistBase, INavigationAssist
    {
        public NavigationAssist(IEventAggregator aggregator, IRegionManager manager)
            : base(aggregator, manager) { }

        public void Add<T>(string regionName, params string[] schemeNames)
        {
            if (!RegionManager.Regions.ContainsRegionWithName(regionName))
                return;

            if (!RegionManager.Regions[regionName].Views.Any(x => x.GetType() == typeof(T)))
                RegionManager.RegisterViewWithRegion(regionName, typeof(T));

            foreach (string schemeName in schemeNames)
            {
                Schemes.Add(new SchemeNavigator(schemeName, regionName, typeof(T).Name));
            }
        }

        protected override void ChangeViews(string schemeName)
        {
            if (schemeName == CurrentScheme)
                return;

            foreach (SchemeNavigator scheme in Schemes.FindAll(x => x.SchemeName == schemeName))
            {
                RegionManager.RequestNavigate(scheme.RegionName, scheme.ViewName);
            }

            CurrentScheme = schemeName;
        }
    }
}
