using Prism.Events;
using Opus.Events.Data;
using System;

namespace Opus.Events
{
    /// <summary>
    /// An event notifying of a bookmark selection.
    /// <para>
    /// Carries information of a bookmark as payload.
    /// </para>
    /// </summary>
    public class BookmarkSelectedEvent : PubSubEvent<BookmarkInfo> { }

    /// <summary>
    /// An event notifying of deselection of a bookmark.
    /// <para>
    /// Guid of the deselected bookmark is sent as payload.
    /// </para>
    /// </summary>
    public class BookmarkDeselectedEvent : PubSubEvent<Guid> { }

    /// <summary>
    /// An event signifying the deletion of a file from the bookmarks list.
    /// <para>
    /// Payload is the filepath of the deleted file.
    /// </para>
    /// </summary>
    public class BookmarkFileDeletedEvent : PubSubEvent<string> { }
}
