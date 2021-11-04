using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PDFLib.Services;

namespace PDFLib.Implementation
{
    public class BookmarkOperator : IBookmarkOperator
    {
        public BookmarkOperator() { }
        public static IBookmarkOperator GetService() => new BookmarkOperator();

        public List<Bookmark> GetBookmarks(string sourceFile)
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
        public List<IBookmark> ImportBookmarks(string sourceFile, int documentEndPage)
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
                    if (char.IsWhiteSpace(c))
                        levelValue++;
                    else
                        break;
                }

                string titleValue = line.Substring(0, line.LastIndexOf(";")).Trim();
                string bmstring = whitespace.Replace(line, "");

                int separatorIndex = bmstring.LastIndexOf(";");
                int pageValue;
                if (!int.TryParse(bmstring.Substring(separatorIndex + 1), out pageValue))
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
        public int GetLastPage(string sourceFile)
        {
            PdfDocument doc = new PdfDocument(new PdfReader(sourceFile));
            int pages = doc.GetNumberOfPages();
            doc.Close();
            return pages;
        }

        private static void ListBookmarks(PdfOutline outline, IDictionary<string, PdfObject> names,
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

            foreach (PdfOutline child in outline.GetAllChildren())
            {
                ListBookmarks(child, names, document, bookmarks, level, parentId);
            }
        }
    }
}
