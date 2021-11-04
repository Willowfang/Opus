using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Office.Interop.Word;
using PDFLib.Services;

namespace PDFLib.Implementation
{
    /// <summary>
    /// Provides Word-file conversion
    /// </summary>
    public class ConvertWord : IConvert
    {
        /// <summary>
        /// Convert a Word-file into a PDF.
        /// </summary>
        /// <param name="filePath">File to convert</param>
        /// <param name="outputDirectory">Directory to save converted file into</param>
        /// <returns>Converted file path. Null, if not a pdf.</returns>
        public string Convert(string filePath, string outputDirectory)
        {
            string[] result = Convert(new string[] { filePath }, outputDirectory);
            if (result.Length > 0) return result[0];
            else return null;
        }

        /// <summary>
        /// Convert Word-files to PDF. Uses the same <see cref="Application"/> for all of the files.
        /// </summary>
        /// <param name="filePaths">Files to convert</param>
        /// <param name="outputDirectory">Directory to save converted files into</param>
        /// <returns>Filepaths of the converted files</returns>
        public string[] Convert(string[] filePaths, string outputDirectory)
        {
            var app = new Application();
            List<string> converted = new List<string>();

            foreach (string inputPath in filePaths)
            {
                var ext = Path.GetExtension(inputPath);
                if (ext != ".doc" && ext != ".docx") continue;

                string outputPath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(inputPath) + ".pdf");
                var doc = app.Documents.Open(inputPath);
                doc.ExportAsFixedFormat(outputPath, WdExportFormat.wdExportFormatPDF);
                doc.Close();
                converted.Add(outputPath);
            }

            return converted.ToArray();
        }
    }
}
