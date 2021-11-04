using PDFLib.ExtensionMethods;
using PDFLib.PDF.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using iText.Kernel.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PDFLib.PDF.Implementation
{
    public class Extraction : IExtraction
    {
        public Extraction() { }
        public static IExtraction GetService() => new Extraction();

        /// <summary>
        /// Extract bookmark-ranges to separate files
        /// </summary>
        /// <param name="sourceFile">Path of the file to extract from</param>
        /// <param name="destDirectory">Directory to extract the files to</param>
        /// <param name="allBookMarks">All the bookmarks in the file. Contains bookmarks that have been selected.</param>
        public void ExtractSeparate(string sourceFile, string destDirectory, IEnumerable<IBookmark> allBookMarks)
        {
            List<IBookmark> selectedBookmarks = allBookMarks.ToList().FindAll(x => x.IsSelected);

            var doc = new PdfDocument(new PdfReader(sourceFile));

            foreach (IBookmark mark in selectedBookmarks)
            {
                HashSet<int> pages = new HashSet<int>();
                for (int i = mark.StartPage; i <= mark.EndPage; i++)
                {
                    pages.Add(i);
                }

                string range = GetRangeString(pages.ToList());

                string fileName = mark.Title.ReplaceIllegal() + ".pdf";

                var split = new ExtSplitter(doc, pageRange => new PdfWriter(Path.Combine(destDirectory, fileName)));
                var result = split.ExtractPageRange(new PageRange(range));

                result.Close();
            }

            doc.Close();
        }
        /// <summary>
        /// Extract bookmark-ranges into a single file
        /// </summary>
        /// <param name="sourceFile">Path of the file to extract from</param>
        /// <param name="destFile">File to extract to</param>
        /// <param name="allBookMarks">All the bookmarks of the file. Contains selected bookmarks.</param>
        public void Extract(string sourceFile, string destFile, IEnumerable<IBookmark> allBookMarks)
        {
            List<IBookmark> selectedBookmarks = allBookMarks.ToList().FindAll(x => x.IsSelected);
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

                Bookmark addition =
                    new Bookmark(current.Title, startpage, current.Id);
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
        public void ExtractCMD(string sourceFile, string destDirectory, string selectPrefix = null)
        {
            List<Bookmark> marks = BookmarkOperator.GetService().GetBookmarks(sourceFile);

            if (!string.IsNullOrEmpty(selectPrefix))
            {
                foreach (Bookmark mark in marks)
                    if (mark.Title.ToLower().StartsWith(selectPrefix.ToLower())) mark.IsSelected = true;
            }
            else
            {
                foreach (Bookmark mark in marks)
                    mark.IsSelected = true;
            }

            ExtractSeparate(sourceFile, destDirectory, marks);
        }

        // Get iText7 extraction compatible string
        private static string GetRangeString(List<int> numbers)
        {
            var ranges = new List<string>();

            if (numbers.Count == 0)
                return null;

            numbers = numbers.Distinct().ToList();
            numbers.Sort();

            int start = numbers[0];
            string range = start.ToString();

            // Add all page ranges to string
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

            // Return a joint string of all ranges
            return string.Join(", ", ranges);
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
        internal class ExtSplitter : PdfSplitter
        {
            private Func<PageRange, PdfWriter> nextWriter;
            internal ExtSplitter(PdfDocument doc, Func<PageRange, PdfWriter> nextWriter) : base(doc)
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
