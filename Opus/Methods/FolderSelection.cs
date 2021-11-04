using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Opus.Methods
{
    public static class FolderSelection
    {
        public static string SelectFolder()
        {
            FolderBrowserDialog browseDialog = new FolderBrowserDialog();
            browseDialog.ShowNewFolderButton = true;

            if (browseDialog.ShowDialog() == DialogResult.Cancel)
                return null;
            else
                return browseDialog.SelectedPath;
        }
    }
}
