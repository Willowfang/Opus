using PDFExtractor.Core.Events;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDFExtractor.Core.Base
{
    public abstract class ViewAssistBase
    {
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
        protected IRegionManager RegionManager;
        protected List<SchemeNavigator> Schemes;
        protected string CurrentScheme;

        public ViewAssistBase(IEventAggregator aggregator, IRegionManager manager)
        {
            Schemes = new List<SchemeNavigator>();
            Aggregator = aggregator;
            RegionManager = manager;
            Aggregator.GetEvent<ViewChangeEvent>().Subscribe(ChangeViews);
        }

        protected abstract void ChangeViews(string schemeName);
    }
}
