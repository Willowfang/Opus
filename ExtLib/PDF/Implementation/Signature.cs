using PDFLib.ExtensionMethods;
using PDFLib.PDF.Services;
using iText.Forms;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PDFLib.PDF.Implementation
{
    public class Signature : ISignature
    {
        public Signature() { }
        public static ISignature GetService() => new Signature();

        /// <summary>
        /// Create a copy of a document, where the digital signature has been removed
        /// </summary>
        /// <param name="file">File to process</param>
        /// <param name="destDirectory">Directory to save the file to</param>
        /// <param name="identifier">Additional identifier for the new file (e.g. "file.pdf" --> "file_nosignature.pdf")</param>
        public void Remove(IFilePDF file, string destDirectory, string identifier)
        {
            Remove(new IFilePDF[] { file }, destDirectory, identifier);
        }
        /// <summary>
        /// Create copies of documents, where the digital signature has been removed
        /// </summary>
        /// <param name="sourceFiles">Files to process</param>
        /// <param name="destDirectory">Directory to save the file to</param>
        /// <param name="identifier">Additional identifier for the new file (e.g. "file.pdf" --> "file_nosignature.pdf")</param>
        public void Remove(IFilePDF[] sourceFiles, string destDirectory, string identifier)
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
        public void RemoveCMD(string filePath, string identifier)
        {
            IFilePDF pdf = new FilePDF(filePath);
            Remove(pdf, Path.GetDirectoryName(filePath), identifier);
        }
    }
}
