using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using iText.Forms;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Navigation;
using iText.Kernel.Utils;
using ExtLib.ExtensionMethods;

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

        public static int GetLastPage(string sourceFile)
        {
            PdfDocument doc = new PdfDocument(new PdfReader(sourceFile));
            int pages = doc.GetNumberOfPages();
            doc.Close();
            return pages;
        }
    }

    public interface IFilePDF
    {
        string Title { get; }
        string FilePath { get; }
        bool IsSelected { get; set; }
    }
    public class FilePDF : BaseClasses.ModelBase, IFilePDF
    {
        public string Title
        {
            get => Path.GetFileNameWithoutExtension(FilePath);
        }
        public string FilePath { get; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public FilePDF(string filePath)
        {
            FilePath = filePath;
        }
    }

    public static class Extraction
    {
        #region IN_DEVELOPMENT

        private static Regex re_charges = new Regex(@"\d\.\s[A-Ö]{3,}");
        private static Regex re_invNumbers = new Regex(@"\(\d+\/[Rr]\/\d+\/\d{2,2}\)");
        private static Regex re_caseNumberOnly = new Regex(@"[1-9]\d+(?=[/]\d{2,})");
        private static Regex re_caseNumberZeros = new Regex(@"[0-9]\d+(?=[/]\d{2,})");
        private static Regex re_evidenceNumber = new Regex(@"(?<!\.)\d+\.(?![0-9])");
        private static Regex re_evidenceParentheses = new Regex(@"\(((.|\n)*?)\)");
        private static Regex re_invNumbersNoParentheses = new Regex(@"\d+\/[Rr]\/\d+\/\d{2,2}");
        private static Regex re_evidencePageNumbers = new Regex(@"(?<=s\.|sivut|sivu)[0-9\-\,\s]+");

        private const string EVIDENCE = "Kirjalliset todisteet";
        private const string PEOPLE = "Henkilötodistelu";
        private const string PROSECUTOR_OTHER = "SYYTTÄJÄN MUUT VAATIMUKSET";
        private const string PLAINTIFF_DEMANDS = "SYYTTÄJÄN AJAMAT ASIANOMISTAJAN YKSITYISOIKEUDELLISET VAATIMUKSET";
        private const string OTHER_INFO = "LISÄTIETOJA";

        private class Charge
        {
            public class ReportFile
            {
                public string FormattedName { get; private set; }
                public string CaseNumber { get; private set; }

                private string filePath;
                public string FilePath
                {
                    get { return filePath; }
                    set
                    {
                        filePath = value;
                        string name = Path.GetFileNameWithoutExtension(value);
                        name = name.Replace("_", "/").Replace("-", "/");
                        CaseNumber = re_caseNumberOnly.Match(name).Value;
                        FormattedName = re_caseNumberZeros.Replace(name, CaseNumber);
                    }
                }
            }
            public class PieceOfEvidence
            {
                public string FullText { get; set; }
                public string Name { get; set; }
                public ReportFile Report { get; set; }
                public int StartPage { get; set; }
                public int EndPage { get; set; }
            }

            public string FullText { get; set; }
            public string EvidenceText { get; set; }
            public string FullInvestigationNumber { get; set; }
            public string CaseNumberOnly { get; set; }
            public List<PieceOfEvidence> Evidence { get; set; }

            public ReportFile MainReport { get; set; }
            public List<ReportFile> AdditionalReports { get; set; }
        }
        
        private static string GetFullText(string sourceFile)
        {
            var pdfDocument = new PdfDocument(new PdfReader(sourceFile));
            var strategy = new SimpleTextExtractionStrategy();

            int endPage = pdfDocument.GetNumberOfPages();

            for (int i = 1; i <= endPage; ++i)
            {
                var page = pdfDocument.GetPage(i);
                PdfTextExtractor.GetTextFromPage(page, strategy);
            }

            return strategy.GetResultantText();
        }

        private static string[] GetChargeHeadings(string fullText)
        {
            return re_charges.Matches(fullText)
                .Cast<Match>()
                .Select(match => match.Value)
                .ToArray();
        }

        private static List<Charge> GetCharges(string fullText, string[] headings)
        {
            List<Charge> charges = new List<Charge>();

            int[] generalIndexes = new int[]
            {
                fullText.IndexOf(PROSECUTOR_OTHER),
                fullText.IndexOf(PLAINTIFF_DEMANDS),
                fullText.IndexOf(OTHER_INFO)
            };

            List<int> startIndexes = new List<int>();
            foreach (string heading in headings)
            {
                startIndexes.Add(fullText.IndexOf(heading));
            }

            for (int i = 0; i < headings.Length; i++)
            {
                string heading = headings[i];
                Charge currentCharge = new Charge();
                int endIndex = 0;
                
                if (i == headings.Length - 1)
                {
                    foreach (int ind in generalIndexes)
                    {
                        if (ind > -1)
                        {
                            endIndex = ind;
                            break;
                        }
                    }
                }
                else
                {
                    endIndex = startIndexes[i + 1];
                }

                currentCharge.FullText = fullText.Substring(startIndexes[i], endIndex - startIndexes[i]);
                currentCharge.FullInvestigationNumber = re_invNumbers.Match(currentCharge.FullText).Value;
                currentCharge.CaseNumberOnly = re_caseNumberOnly.Match(currentCharge.FullInvestigationNumber).Value;

                charges.Add(currentCharge);
            }

            return charges;
        }

        private static void GetFilesForCharges(List<Charge> charges, string folderPath)
        {
            string[] allFiles = Directory.GetFiles(folderPath);

            foreach (Charge c in charges)
            {
                List<Charge.ReportFile> relatedFiles = new List<Charge.ReportFile>();

                foreach (string file in allFiles)
                {
                    string extension = Path.GetExtension(file).ToLower();
                    string name = Path.GetFileNameWithoutExtension(file).ToLower();

                    if (extension != ".pdf")
                        continue;
                    if (!re_invNumbersNoParentheses.IsMatch(name.Replace("_", "/").Replace("-", "/")))
                        continue;
                    if (!name.Contains(c.CaseNumberOnly))
                        continue;
                    if (name.EndsWith("ht") || name.EndsWith("htl") || name.EndsWith("henkilötietolehti") )
                        continue;

                    relatedFiles.Add(new Charge.ReportFile() { FilePath = file });
                }

                Charge.ReportFile shortest = relatedFiles[0];
                foreach (Charge.ReportFile s in relatedFiles)
                {
                    if (s.FormattedName.Length < shortest.FormattedName.Length) shortest = s;
                }
                c.MainReport = shortest;
                relatedFiles.Remove(shortest);

                foreach (Charge.ReportFile report in relatedFiles)
                {
                    string fileNameLower = Path.GetFileNameWithoutExtension(report.FilePath).ToLower();
                    if (fileNameLower.Contains("li") || fileNameLower.Contains("ltptk") || fileNameLower.Contains("lisätutkinta"))
                        c.AdditionalReports.Add(report);
                }
            }
        }

        private static void GetEvidence(Charge charge, List<Charge> charges)
        {
            int evidStartPosition = 0;
            int evidEndPosition = 0;
            List<Charge.PieceOfEvidence> evidence = new List<Charge.PieceOfEvidence>();

            evidStartPosition = charge.FullText.IndexOf(EVIDENCE);
            if (evidStartPosition == -1)
                return;

            evidStartPosition = evidStartPosition + EVIDENCE.Length;

            evidEndPosition = charge.FullText.IndexOf(PEOPLE);
            if (evidEndPosition == -1)
                evidEndPosition = charge.FullText.Length - 1;

            charge.EvidenceText = charge.FullText.Substring(evidStartPosition, evidEndPosition - evidStartPosition);

            int[] piecePositions = re_evidenceNumber.Matches(charge.EvidenceText)
                .Cast<Match>()
                .Select(match => match.Index)
                .ToArray();

            for (int i = 0; i < piecePositions.Length; i++)
            {
                Charge.PieceOfEvidence piece = new Charge.PieceOfEvidence();
                if (i == piecePositions.Length - 1)
                    piece.FullText = charge.EvidenceText.Substring(piecePositions[i], charge.EvidenceText.Length - 1 - piecePositions[i]);
                else
                    piece.FullText = charge.EvidenceText.Substring(piecePositions[i], piecePositions[i + 1] - piecePositions[i]);

                evidence.Add(piece);
            }

            foreach (Charge.PieceOfEvidence piece in evidence)
            {
                Match parentheses = re_evidenceParentheses.Match(piece.FullText);
                Match number = re_evidenceNumber.Match(piece.FullText);
                int whitespaces = number.Value.Count(x => char.IsWhiteSpace(x));
                int startPosition = number.Index + number.Value.Length + whitespaces;
                int endPosition = parentheses.Index - startPosition;

                piece.Name = piece.FullText.Substring(startPosition, endPosition).Trim();

                string pages = re_evidencePageNumbers.Match(parentheses.Value).Value.Trim();

                int startPage = 0;
                int endPage = 0;
                if (!pages.Contains("-"))
                    Int32.TryParse(pages, out startPage);
                else
                {
                    string[] startEnd = pages.Split('-');
                    if (startEnd.Length == 2)
                    {
                        bool startSuccess = false;
                        int startP = 0;
                        bool endSuccess = false;
                        int endP = 0;

                        startSuccess = Int32.TryParse(startEnd[0], out startP);
                        endSuccess = Int32.TryParse(startEnd[1], out endP);

                        if (startSuccess && endSuccess)
                        {
                            startPage = startP;
                            endPage = endP;
                        }
                    }
                }

                piece.StartPage = startPage;
                piece.EndPage = endPage;

                string parLow = parentheses.Value.ToLower();

                Charge.ReportFile report = null;
                Match otherReport = re_invNumbersNoParentheses.Match(parentheses.Value);
                string caseNumber = re_caseNumberOnly.Match(otherReport.Value).Value;
                bool additionalReport = parLow.Contains("ltpkt") || parLow.Contains("lisätutkinta");

                if (otherReport.Success)
                {
                    foreach (Charge c in charges)
                    {
                        if (!additionalReport)
                        {
                            if (c.MainReport.CaseNumber == caseNumber)
                            {
                                report = c.MainReport;
                                break;
                            }
                        }
                        else
                        {
                            foreach (Charge.ReportFile r in c.AdditionalReports)
                            {
                                if (r.CaseNumber == caseNumber)
                                {
                                    report = r;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (additionalReport)
                        report = charge.AdditionalReports.Find(x => x.CaseNumber == caseNumber);
                    else
                        report = charge.MainReport;
                }

                piece.Report = report;

                charge.Evidence.Add(piece);
            }
        }

        #endregion

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

        /// <summary>
        /// Extract bookmark-ranges to separate files
        /// </summary>
        /// <param name="sourceFile">Path of the file to extract from</param>
        /// <param name="destDirectory">Directory to extract the files to</param>
        /// <param name="allBookMarks">All the bookmarks in the file. Contains bookmarks that have been selected.</param>
        public static void ExtractSeparate(string sourceFile, string destDirectory, IEnumerable<IBookmark> allBookMarks)
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

    public static class Signature
    {
        /// <summary>
        /// Create a copy of a document, where the digital signature has been removed
        /// </summary>
        /// <param name="file">File to process</param>
        /// <param name="destDirectory">Directory to save the file to</param>
        /// <param name="identifier">Additional identifier for the new file (e.g. "file.pdf" --> "file_nosignature.pdf")</param>
        public static void Remove(IFilePDF file, string destDirectory, string identifier)
        {
            Remove(new IFilePDF[] { file }, destDirectory, identifier);
        }
        // <summary>
        /// Create copies of documents, where the digital signature has been removed
        /// </summary>
        /// <param name="sourceFiles">Files to process</param>
        /// <param name="destDirectory">Directory to save the file to</param>
        /// <param name="identifier">Additional identifier for the new file (e.g. "file.pdf" --> "file_nosignature.pdf")</param>
        public static void Remove(IFilePDF[] sourceFiles, string destDirectory, string identifier)
        {
            foreach (IFilePDF file in sourceFiles)
            {
                string fileName = file.Title + "_" + identifier + ".pdf";
                string destFile = Path.Combine(destDirectory, fileName.ReplaceIllegal());
                PdfDocument doc = new PdfDocument(new PdfReader(file.FilePath), new PdfWriter(destFile));
                PdfAcroForm form = PdfAcroForm.GetAcroForm(doc, true);
                form.FlattenFields();
                doc.Close();
            }
        }
    }
}
