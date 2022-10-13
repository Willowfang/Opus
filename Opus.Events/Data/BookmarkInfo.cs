using System;

namespace Opus.Events.Data
{
    public class BookmarkInfo
    {
        public int StartPage { get; }
        public int EndPage { get; }
        public string Title { get; }
        public string FilePath { get; }
        public Guid Id { get; }

        public BookmarkInfo(int startPage, int endPage, string title, string filePath, Guid id = default)
        {
            StartPage = startPage;
            EndPage = endPage;
            Title = title;
            FilePath = filePath;
            Id = id;
        }
    }
}
