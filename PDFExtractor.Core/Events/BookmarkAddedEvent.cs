using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;
using ExtLib;

namespace PDFExtractor.Core.Events
{
    public class BookmarkAddedEvent : PubSubEvent<IBookmark>
    {
    }
}
