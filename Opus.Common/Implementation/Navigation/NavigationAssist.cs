using WF.LoggingLib;
using Opus.Events;
using Opus.Common.Logging;
using Opus.Common.Services.Navigation;
using Prism.Events;
using Prism.Regions;

namespace Opus.Common.Implementation.Navigation
{
    /// <summary>
    /// A default implementation of <see cref="INavigationAssist"/>.
    /// </summary>
    public class NavigationAssist
        : LoggingCapable<NavigationAssist>,
            INavigationAssist,
            INavigationTargetRegistry
    {
        /// <summary>
        /// A class for storing information about a viewModel and its association with a scheme.
        /// </summary>
        protected class SchemeTarget
        {
            /// <summary>
            /// Scheme stored for the viewModel.
            /// </summary>
            public string SchemeName { get; }

            /// <summary>
            /// Target of the scheme navigation.
            /// </summary>
            public INavigationTarget Target { get; }

            /// <summary>
            /// Subscription token with which to cancel the subscription to reset event.
            /// </summary>
            public SubscriptionToken? ResetEventToken { get; set; }

            /// <summary>
            /// Create a new scheme target.
            /// </summary>
            /// <param name="schemeName">Name of the scheme to apply.</param>
            /// <param name="target">Viewmodel target for scheme.</param>
            public SchemeTarget(string schemeName, INavigationTarget target)
            {
                SchemeName = schemeName;
                Target = target;
            }
        }

        /// <summary>
        /// Navigator class associating view, scheme and region together.
        /// </summary>
        protected class SchemeNavigator
        {
            /// <summary>
            /// Name of the scheme associated with this instance.
            /// </summary>
            public string SchemeName { get; }

            /// <summary>
            /// Name of the region associated with this instance.
            /// </summary>
            public string RegionName { get; }

            /// <summary>
            /// Name of the view associated with this instance.
            /// </summary>
            public string ViewName { get; }

            /// <summary>
            /// Create a new scheme navigator instance.
            /// </summary>
            /// <param name="schemeName">Name of the scheme to apply.</param>
            /// <param name="regionName">Name of the region to register.</param>
            /// <param name="viewName">Name of the view to register.</param>
            public SchemeNavigator(string schemeName, string regionName, string viewName)
            {
                SchemeName = schemeName;
                RegionName = regionName;
                ViewName = viewName;
            }
        }

        /// <summary>
        /// Event publish and subscription service.
        /// </summary>
        protected IEventAggregator Aggregator;

        /// <summary>
        /// Service for managing regions and their navigation.
        /// </summary>
        protected IRegionManager Manager;

        /// <summary>
        /// List of registered schemes and their associations.
        /// </summary>
        protected List<SchemeNavigator> Schemes;

        /// <summary>
        /// List of registered navigation targets for schemes.
        /// </summary>
        protected List<SchemeTarget> Targets;

        /// <summary>
        /// Currently selected scheme.
        /// </summary>
        protected string? CurrentScheme;

        /// <summary>
        /// Create a new navigation assistant.
        /// </summary>
        /// <param name="aggregator">Event publish and subscribe service.</param>
        /// <param name="manager">Region manager service.</param>
        /// <param name="logbook">Logging service.</param>
        public NavigationAssist(
            IEventAggregator aggregator,
            IRegionManager manager,
            ILogbook logbook
        ) : base(logbook)
        {
            Aggregator = aggregator;
            Manager = manager;
            Schemes = new List<SchemeNavigator>();
            Targets = new List<SchemeTarget>();

            aggregator.GetEvent<ViewChangeEvent>().Subscribe(ChangeViews);
        }

        /// <summary>
        /// Associate a region with scheme names and a view.
        /// </summary>
        /// <typeparam name="TView">Type of the view.</typeparam>
        /// <param name="regionName">Name of the region to apply navigation to.</param>
        /// <param name="schemeNames">Names of the schemes to associate with this navigation.</param>
        public void Add<TView>(string regionName, params string[] schemeNames)
        {
            logbook.Write(
                $"Associating {nameof(TView)} view and region '{regionName}' with following schemes: {schemeNames}.",
                LogLevel.Debug);

            if (!Manager.Regions.ContainsRegionWithName(regionName))
            {
                logbook.Write($"No region named '{regionName}' found.", LogLevel.Debug);

                return;
            }

            if (!Manager.Regions[regionName].Views.Any(x => x.GetType() == typeof(TView)))
                Manager.RegisterViewWithRegion(regionName, typeof(TView));

            foreach (string schemeName in schemeNames)
            {
                Schemes.Add(new SchemeNavigator(schemeName, regionName, typeof(TView).Name));
            }

            logbook.Write($"View, region and schemes registered.", LogLevel.Debug);
        }

        /// <summary>
        /// Add a navigation target.
        /// </summary>
        /// <param name="schemeName">Name of the scheme to associate with.</param>
        /// <param name="target">Target of the navigation.</param>
        public void AddTarget(string schemeName, INavigationTarget target)
        {
            Targets.Add(new SchemeTarget(schemeName, target));
        }

        /// <summary>
        /// Change current views for all regions.
        /// </summary>
        /// <param name="schemeName">Name of the scheme to switch to.</param>
        protected void ChangeViews(string schemeName)
        {
            logbook.Write($"Changing navigation scheme to '{schemeName}'.", LogLevel.Debug);

            if (schemeName == CurrentScheme)
            {
                logbook.Write($"Scheme already selected.", LogLevel.Debug);
                return;
            }

            // Request navigation for given scheme for all associated regions and views.

            Schemes
                .FindAll(scheme => scheme.SchemeName == schemeName)
                .ForEach(x => Manager.RequestNavigate(x.RegionName, x.ViewName));

            // Subscribe to events and fire navigation related methods for viewmodels that were
            // navigated to. Remove those event subscriptions from viewmodels that were navigated
            // from and fire their exit-methods.

            Targets
                .FindAll(target => target.SchemeName == schemeName)
                .ForEach(x =>
                {
                    x.ResetEventToken = Aggregator
                        .GetEvent<ActionResetEvent>()
                        .Subscribe(x.Target.Reset);

                    x.Target.OnArrival();
                });

            Targets
                .FindAll(target => target.SchemeName == CurrentScheme)
                .ForEach(x =>
                {
                    Aggregator.GetEvent<ActionResetEvent>().Unsubscribe(x.ResetEventToken);

                    x.Target.WhenLeaving();
                });

            CurrentScheme = schemeName;

            logbook.Write($"Navigation scheme changed.", LogLevel.Debug);
        }
    }
}
