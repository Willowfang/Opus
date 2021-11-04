using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PDFLib.Internal
{
    internal static class ExtractionDEPRECATED
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
                    if (name.EndsWith("ht") || name.EndsWith("htl") || name.EndsWith("henkilötietolehti"))
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

    }
}
