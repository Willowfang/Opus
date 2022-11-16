using Opus.Actions.Services.Base;
using Opus.Common.ViewModels;
using Opus.Common.Services.Commands.Base;
using WF.LoggingLib;

namespace Opus.Actions.Base
{
    /// <summary>
    /// Base viewmodel class for action viewmodels. Inherits <see cref="ViewModelBaseLogging{ViewModelType}"/>.
    /// </summary>
    /// <typeparam name="VMType">Type of the viewmodel inheriting this class.</typeparam>
    /// <typeparam name="TProps">Properties service for inheriting viewmodel.</typeparam>
    /// <typeparam name="TEvents">Events service for inheriting viewmodel.</typeparam>
    /// <typeparam name="TCommands">Commands service for inheriting viewmodel.</typeparam>
    public abstract class ActionViewModelBase<VMType, TProps, TEvents, TCommands>
        : ViewModelBaseLogging<VMType> 
        where TProps : IActionProperties
        where TEvents : IActionEventHandling
        where TCommands : IActionCommands
    {
        /// <summary>
        /// Properties for this action viewmodel.
        /// </summary>
        public TProps Properties { get; }

        /// <summary>
        /// Event handling for this action viewmodel.
        /// </summary>
        public TEvents Events { get; }

        /// <summary>
        /// Commands for this action viewmodel.
        /// </summary>
        public TCommands Commands { get; }

        /// <summary>
        /// Base viewmodel for action viewmodels.
        /// </summary>
        /// <param name="properties">Properties for this viewmodel.</param>
        /// <param name="events">Event handling for this viewmodel.</param>
        /// <param name="commands">Command for this viewmodel.</param>
        /// <param name="logbook">Logging service.</param>
        public ActionViewModelBase(
            TProps properties,
            TEvents events,
            TCommands commands,
            ILogbook logbook) : base(logbook)
        {
            Properties = properties;
            Events = events;
            Commands = commands;
        }
    }
}
