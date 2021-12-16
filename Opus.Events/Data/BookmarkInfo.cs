namespace Opus.Events.Data
{
    public class BookmarkInfo
    {
        public int StartPage { get; }
        public int EndPage { get; }
        public string Title { get; }

        public BookmarkInfo(int startPage, int endPage, string title)
        {
            StartPage = startPage;
            EndPage = endPage;
            Title = title;
        }
    }
}
