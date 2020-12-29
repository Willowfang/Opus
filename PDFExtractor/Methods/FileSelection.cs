using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace PDFExtractor.Methods
{
    public static class FileSelection
    {
        public static string OpenFilePathSelection()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = Messages.OpenFileTitle;
            openDialog.Filter = "PDF (.pdf)|*.pdf";

            if (openDialog.ShowDialog() == true)
                return openDialog.FileName;
            else
                return null;
        }
        public static string SaveFilePathSelection(string initialDirectory)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = Messages.SaveFileTitle;
            saveDialog.Filter = "PDF (.pdf)|*.pdf";
            saveDialog.InitialDirectory = initialDirectory;

            if (saveDialog.ShowDialog() == true)
                return saveDialog.FileName;
            else
                return null;
        }
    }
}
