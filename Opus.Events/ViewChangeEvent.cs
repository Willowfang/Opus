using Prism.Events;

namespace Opus.Events
{
    /// <summary>
    /// An event notifying of the change of the view scheme.
    /// <para>
    /// Has the name of the scheme as payload.
    /// </para>
    /// </summary>
    public class ViewChangeEvent : PubSubEvent<string> { }
}
