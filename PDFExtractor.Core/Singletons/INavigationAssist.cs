using PDFExtractor.Core.Base;
using PDFExtractor.Core.Events;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDFExtractor.Core.Singletons
{
    public interface INavigationAssist
    {
        public void Add<T>(string regionName, string schemeName);
    }

    public class NavigationAssist : ViewAssistBase, INavigationAssist
    {
        public NavigationAssist(IEventAggregator aggregator, IRegionManager manager)
            : base(aggregator, manager) { }

        public void Add<T>(string regionName, string schemeName)
        {
            RegionManager.RegisterViewWithRegion(regionName, typeof(T));
            Schemes.Add(new SchemeNavigator(schemeName, regionName, typeof(T).Name));
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
