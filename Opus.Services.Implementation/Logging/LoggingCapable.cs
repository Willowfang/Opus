using CX.LoggingLib;
using LoggingLib.Defaults;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Logging
{
    /// <summary>
    /// A class that is capable of logging to file.
    /// </summary>
    /// <typeparam name="T">Type of the implementing class</typeparam>
    public abstract class LoggingCapable<T> : BindableBase
    {
        /// <summary>
        /// Base logbook.
        /// </summary>
        protected TypedLogbook<T> logbook;

        /// <summary>
        /// Create new logging capable instance of the class.
        /// </summary>
        /// <param name="logbook">Base logbook for logging actions.</param>
        public LoggingCapable(ILogbook logbook)
        {
            if (logbook == null)
                this.logbook = EmptyLogbook.Create<T>();
            else
                this.logbook = logbook.CreateTyped<T>();
        }

        /// <summary>
        /// Change the current logbook.
        /// </summary>
        /// <param name="logbook">New logbook.</param>
        public virtual void ChangeLogbook(ILogbook logbook)
        {
            this.logbook = logbook.CreateTyped<T>();
        }
    }
}
