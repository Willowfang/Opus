using Prism.Events;

namespace Opus.Events
{
    /// <summary>
    /// An event signifying a file has been selected and added by the user.
    /// <para>
    /// The payload is the filepath of the added file.
    /// </para>
    /// </summary>
    public class FilesAddedEvent : PubSubEvent<string[]> { }
}
