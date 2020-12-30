using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using iText.Kernel.Utils;

namespace ExtLib
{
    public interface IBookmark
    {
        Guid Id { get; }
        Guid ParentId { get; set; }

        string Title { get; }
        int StartPage { get; set; }
        int EndPage { get; set; }
        int Level { get; set; }
        string Range { get; }
        bool IsSelected { get; set; }
    }

    public static class Bookmarks
    {
        public class Bookmark : BaseClasses.ModelBase, IBookmark
        {
            public Guid Id { get; }
            public Guid ParentId { get; set; }

            public string Title { get; set; }
            public int StartPage { get; set; }
            public int EndPage { get; set; }
            public int Level { get; set; }

            public string Range
            {
                get
                {
                    if (StartPage == EndPage)
                        return StartPage.ToString();
                    else
                        return string.Format("{0}-{1}", StartPage.ToString(), EndPage.ToString());
                }
            }

            private bool isSelected;
            public bool IsSelected
            {
                get { return isSelected; }
                set { SetProperty(ref isSelected, value); }
            }

            public Bookmark(string title, int pageNum)
            {
                Title = title;
                StartPage = pageNum;
                Id = Guid.NewGuid();
            }
            public Bookmark(string title, int pageNum, Guid id)
                : this(title, pageNum)
            {
                Id = id;
            }
            public Bookmark(string title, Guid id)
            {
                Title = title;
                Id = id;
            }
        }

        public static List<Bookmark> GetBookmarks(string sourceFile)
        {
            PdfDocument doc = new PdfDocument(new PdfReader(sourceFile));

            PdfNameTree destsTree = doc.GetCatalog().GetNameTree(PdfName.Dests);
            PdfOutline outlines = doc.GetOutlines(false);
            List<Bookmark> bookmarks = new List<Bookmark>();

            if (outlines != null)
            {
                ListBookmarks(outlines, destsTree.GetNames(), doc, bookmarks, 0, Guid.Empty);
            }

            for (int i = 0; i < bookmarks.Count; i++)
            {
                Bookmark mark = bookmarks[i];
                Bookmark leveled = bookmarks.FirstOrDefault(x 
                    => x.Level == mark.Level && 
                    bookmarks.IndexOf(x) > i &&
                    x.ParentId == mark.ParentId);

                if (leveled != null)
                {
                    mark.EndPage = leveled.StartPage - 1;
                }
                else
                {
                    Bookmark levelUp = bookmarks.FirstOrDefault(x => x.Level < mark.Level && bookmarks.IndexOf(x) > i);
                    if (levelUp != null)
                        mark.EndPage = levelUp.StartPage - 1;
                    else
                        mark.EndPage = doc.GetPageNumber(doc.GetLastPage());
                }
            }

            doc.Close();

            return bookmarks;
        }

        private static void ListBookmarks(PdfOutline outline, IDictionary<String, PdfObject> names, 
            PdfDocument document, List<Bookmark> bookmarks, int level, Guid parentId)
        {
            Bookmark mark = null;

            if (outline.GetDestination() != null)
            {
                mark = new Bookmark(outline.GetTitle(),
                    document.GetPageNumber((PdfDictionary)outline.GetDestination().GetDestinationPage(names)));
                mark.Level = level;
                mark.ParentId = parentId;
                bookmarks.Add(mark);

                parentId = mark.Id;
            }

            level++;

            foreach(PdfOutline child in outline.GetAllChildren())
            {
                ListBookmarks(child, names, document, bookmarks, level, parentId);
            }
        }

        public static List<IBookmark> ImportBookmarks(string sourceFile, int documentEndPage)
        {
            Regex whitespace = new Regex(@"\s+");
            List<IBookmark> bookmarks = new List<IBookmark>();

            foreach (string line in File.ReadAllLines(sourceFile))
            {
                if (!line.Contains(";"))
                    continue;

                int levelValue = 1;
                foreach (char c in line)
                {
                    if (Char.IsWhiteSpace(c))
                        levelValue++;
                    else
                        break;
                }

                string titleValue = line.Substring(0, line.LastIndexOf(";")).Trim();
                string bmstring = whitespace.Replace(line, "");

                int separatorIndex = bmstring.LastIndexOf(";");
                int pageValue;
                if (!Int32.TryParse(bmstring.Substring(separatorIndex + 1), out pageValue))
                    continue;
                if (pageValue > documentEndPage)
                    continue;

                Bookmark bm = new Bookmark(titleValue, pageValue);
                bm.Level = levelValue;

                if (bookmarks.Count > 0)
                {
                    for (int i = bookmarks.Count - 1; i > -1; i--)
                    {
                        if (bookmarks[i].EndPage == 0 && bookmarks[i].Level >= bm.Level)
                            bookmarks[i].EndPage = bm.StartPage - 1;

                        if (bookmarks[i].Level < bm.Level)
                        {
                            bm.ParentId = bookmarks[i].Id;
                            break;
                        }
                    }
                }

                bookmarks.Add(bm);
            }

            foreach (Bookmark mark in bookmarks)
            {
                if (mark.EndPage == 0)
                    mark.EndPage = documentEndPage;
            }

            return bookmarks;
        }
    }

    public static class Extraction
    {
        private static string GetRangeString(List<int> numbers)
        {
            var ranges = new List<string>();

            if (numbers.Count == 0)
                return null;

            numbers = numbers.Distinct().ToList();
            numbers.Sort();

            int start = numbers[0];
            string range = start.ToString();

            for (int i = 1; i <= numbers.Count; i++)
            {
                if (i < numbers.Count && numbers[i] == numbers[i - 1] + 1)
                {
                    range = $"{start}-{numbers[i]}";
                    continue;
                }

                ranges.Add(range);

                if (i < numbers.Count)
                {
                    start = numbers[i];
                    range = start.ToString();
                }
            }

            return string.Join(", ", ranges);
        }

        public static void Extract(string sourceFile, string destFile, IEnumerable<IBookmark> allBookMarks)
        {
            List<IBookmark> selectedBookmarks =  allBookMarks.ToList().FindAll(x => x.IsSelected);
            HashSet<int> pages = new HashSet<int>();

            foreach (IBookmark mark in selectedBookmarks)
            {
                for (int i = mark.StartPage; i <= mark.EndPage; i++)
                {
                    pages.Add(i);
                }
            }

            string range = GetRangeString(pages.ToList());

            List<IBookmark> AddMarks = new List<IBookmark>();
            for (int i = 0; i < selectedBookmarks.Count; i++)
            {
                int startpage;
                IBookmark current = selectedBookmarks[i];

                startpage = 1 + pages.ToList().FindAll(x => x < current.StartPage).Count;

                Bookmarks.Bookmark addition = 
                    new Bookmarks.Bookmark(current.Title, startpage, current.Id);
                addition.ParentId = FindClosestParent(allBookMarks.ToList(), selectedBookmarks, current.ParentId);
                AddMarks.Add(addition);
            }

            var doc = new PdfDocument(new PdfReader(sourceFile));

            var split = new ExtSplitter(doc, pageRange => new PdfWriter(destFile));
            var result = split.ExtractPageRange(new PageRange(range));
            result.GetOutlines(true).RemoveOutline();
            result.InitializeOutlines();
            AddRecursively(AddMarks, result, result.GetOutlines(true));

            doc.Close();
            result.Close();
        }

        private static void AddRecursively(List<IBookmark> addedMarks, PdfDocument doc,
            PdfOutline outlineParent, IBookmark bookmarkParent = null)
        {
            if (bookmarkParent != null)
            {
                foreach (IBookmark current in addedMarks.FindAll(x => x.ParentId == bookmarkParent.Id))
                {
                    var subParent = outlineParent.AddOutline(current.Title);
                    subParent.AddDestination(PdfExplicitDestination.CreateFit(doc.GetPage(current.StartPage)));
                    AddRecursively(addedMarks, doc, subParent, current);
                }
            }
            else
            {
                foreach (IBookmark current in addedMarks.FindAll(x => x.ParentId == Guid.Empty))
                {
                    var subParent = outlineParent.AddOutline(current.Title);
                    subParent.AddDestination(PdfExplicitDestination.CreateFit(doc.GetPage(current.StartPage)));
                    AddRecursively(addedMarks, doc, subParent, current);
                }
            }
        }

        private static Guid FindClosestParent(List<IBookmark> allBookmarks, List<IBookmark> selected, 
            Guid parentId)
        {
            if (parentId == Guid.Empty) return parentId;

            // See if current parent is included in the selected bookmarks
            if (selected.Any(x => x.Id == parentId))
                return parentId;

            // Id of parent of parent
            parentId = allBookmarks.Find(x => x.Id == parentId).ParentId;

            return FindClosestParent(allBookmarks, selected, parentId);
        }

        public class ExtSplitter : PdfSplitter
        {
            private Func<PageRange, PdfWriter> nextWriter;
            public ExtSplitter(PdfDocument doc, Func<PageRange, PdfWriter> nextWriter) : base(doc)
            {
                this.nextWriter = nextWriter;
            }

            protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange)
            {
                return nextWriter.Invoke(documentPageRange);
            }
        }
    }
}
