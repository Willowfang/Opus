using System.Collections.Generic;
using System.Linq;
using Opus.Events;
using Opus.Services.UI;
using Prism.Events;
using Prism.Regions;

namespace Opus.Services.Implementation.UI
{
    public class NavigationAssist : INavigationAssist, INavigationTargetRegistry
    {
        protected class SchemeTarget
        {
            public string SchemeName { get; }
            public INavigationTarget Target { get; }

            public SchemeTarget(string schemeName, INavigationTarget target)
            {
                SchemeName = schemeName;
                Target = target;
            }
        }
        protected class SchemeNavigator
        {
            public string SchemeName { get; }
            public string RegionName { get; }
            public string ViewName { get; }

            public SchemeNavigator(string schemeName, string regionName, string viewName)
            {
                SchemeName = schemeName;
                RegionName = regionName;
                ViewName = viewName;
            }
        }

        protected IEventAggregator Aggregator;
        protected IRegionManager Manager;
        protected List<SchemeNavigator> Schemes;
        protected List<SchemeTarget> Targets;
        protected string? CurrentScheme;
             
        public NavigationAssist(IEventAggregator aggregator, IRegionManager manager)
        {
            Aggregator = aggregator;
            Manager = manager;
            Schemes = new List<SchemeNavigator>();
            Targets = new List<SchemeTarget>();
            aggregator.GetEvent<ViewChangeEvent>().Subscribe(ChangeViews);
        }

        public void Add<TView>(string regionName, params string[] schemeNames)
        {
            if (!Manager.Regions.ContainsRegionWithName(regionName))
                return;

            if (!Manager.Regions[regionName].Views.Any(x => x.GetType() == typeof(TView)))
                Manager.RegisterViewWithRegion(regionName, typeof(TView));

            foreach (string schemeName in schemeNames)
            {
                Schemes.Add(new SchemeNavigator(schemeName, regionName, typeof(TView).Name));
            }
        }

        public void AddTarget(string schemeName, INavigationTarget target)
        {
            Targets.Add(new SchemeTarget(schemeName, target));
        }
        
        protected void ChangeViews(string schemeName)
        {
            if (schemeName == CurrentScheme)
                return;

            Schemes.FindAll(scheme => scheme.SchemeName == schemeName).ForEach(x =>
                Manager.RequestNavigate(x.RegionName, x.ViewName));
            Targets.FindAll(target => target.SchemeName == schemeName).ForEach(x => x.Target.OnArrival());
            Targets.FindAll(target => target.SchemeName == CurrentScheme).ForEach(x => x.Target.WhenLeaving());

            CurrentScheme = schemeName;
        }
    }
}
