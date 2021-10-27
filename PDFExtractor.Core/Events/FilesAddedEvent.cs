using Prism.Events;

namespace PDFExtractor.Core.Events
{
    public class FilesAddedEvent : PubSubEvent<string[]> { }
}
