using System.Collections.Generic;
using System.Linq;
using CX.LoggingLib;
using Opus.Events;
using Opus.Services.Implementation.Logging;
using Opus.Services.UI;
using Prism.Events;
using Prism.Regions;

namespace Opus.Services.Implementation.UI
{
    public class NavigationAssist : LoggingCapable<NavigationAssist>, INavigationAssist, INavigationTargetRegistry
    {
        protected class SchemeTarget
        {
            public string SchemeName { get; }
            public INavigationTarget Target { get; }
            public SubscriptionToken? ResetEventToken { get; set; }

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
             
        public NavigationAssist(IEventAggregator aggregator, IRegionManager manager, ILogbook logbook)
            : base(logbook)
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

            logbook.Write($"Region '{regionName}' registered with navigation schemes" + "'{@SchemeNames}'.", LogLevel.Debug,
                customContent: schemeNames);
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
            Targets.FindAll(target => target.SchemeName == schemeName).ForEach(x =>
            {
                x.ResetEventToken = Aggregator.GetEvent<ActionResetEvent>().Subscribe(x.Target.Reset);
                x.Target.OnArrival();
            });
            Targets.FindAll(target => target.SchemeName == CurrentScheme).ForEach(x =>
            {
                Aggregator.GetEvent<ActionResetEvent>().Unsubscribe(x.ResetEventToken);
                x.Target.WhenLeaving();
            });

            CurrentScheme = schemeName;

            logbook.Write($"View navigated to scheme '{schemeName}'.", LogLevel.Debug);
        }
    }
}
