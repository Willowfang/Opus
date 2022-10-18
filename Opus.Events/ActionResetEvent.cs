using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Events
{
    /// <summary>
    /// An event signalling the reset button has been pressed by the user.
    /// <para>
    /// Carries no payload.
    /// </para>
    /// </summary>
    public class ActionResetEvent : PubSubEvent { }
}
