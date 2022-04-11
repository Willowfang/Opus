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
    public abstract class LoggingCapable<T> : BindableBase
    {
        protected TypedLogbook<T> logbook;

        public LoggingCapable(ILogbook logbook)
        {
            if (logbook == null)
                this.logbook = EmptyLogbook.Create<T>();
            else
                this.logbook = logbook.CreateTyped<T>();
        }

        public virtual void ChangeLogbook(ILogbook logbook)
        {
            this.logbook = logbook.CreateTyped<T>();
        }
    }
}
