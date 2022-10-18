using Prism.Events;

namespace Opus.Events
{
    /// <summary>
    /// An event notifying of a selection of a file.
    /// <para>
    /// Has the filepath of the selected file as a payload.
    /// </para>
    /// </summary>
    public class FileSelectedEvent : PubSubEvent<string> { }
}
