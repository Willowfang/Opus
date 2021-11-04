using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;
using PDFLib.Services;

namespace Opus.Core.Events
{
    public class BookmarkAddedEvent : PubSubEvent<IBookmark>
    {
    }
}
