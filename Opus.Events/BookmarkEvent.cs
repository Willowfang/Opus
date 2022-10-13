using Prism.Events;
using Opus.Events.Data;
using System;

namespace Opus.Events
{
    public class BookmarkSelectedEvent : PubSubEvent<BookmarkInfo>{ }

    public class BookmarkDeselectedEvent : PubSubEvent<Guid> { }

    public class BookmarkFileDeletedEvent: PubSubEvent<string> { }
}
