using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Events
{
    /// <summary>
    /// An event signifying that the user has selected a directory path.
    /// <para>
    /// The selected path is sent as the payload.
    /// </para>
    /// </summary>
    public class DirectorySelectedEvent : PubSubEvent<string> { }
}
