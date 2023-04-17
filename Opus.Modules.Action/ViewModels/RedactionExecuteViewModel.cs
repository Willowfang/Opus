using Opus.Actions.Base;
using Opus.Actions.Services.Redact;
using Opus.Common.Services.Commands;
using Opus.Common.Services.Configuration;
using Opus.Common.Services.Navigation;
using Opus.Values;
using System.ComponentModel;
using WF.LoggingLib;

namespace Opus.Modules.Action.ViewModels
{
    /// <summary>
    /// A view model redaction execution.
    /// </summary>
    public class RedactionExecuteViewModel : 
        ActionViewModelBase<
            RedactionExecuteViewModel,
            IRedactProperties,
            IRedactEventHandling,
            IRedactCommands>, INavigationTarget
    {
        /// <summary>
        /// Application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="properties"></param>
        /// <param name="events"></param>
        /// <param name="commands"></param>
        /// <param name="logbook"></param>
        /// <param name="configuration">Application configuration.</param>
        public RedactionExecuteViewModel(
            INavigationTargetRegistry registry,
            IRedactProperties properties,
            IRedactEventHandling events,
            IRedactCommands commands,
            ILogbook logbook,
            IConfiguration configuration)
            : base(properties, events, commands, logbook)
        {
            registry.AddTarget(SchemeNames.REDACT, this);
            Configuration = configuration;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnArrival() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Reset() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void WhenLeaving() { }
    }
}
