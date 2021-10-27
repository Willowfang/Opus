using System;
using System.Collections.Generic;
using System.Text;

namespace PDFExtractor.Core.Constants
{
    public static class RegionNames
    {
        public const string SHELL_FILE = "ShellFileRegion";
        public const string SHELL_BOOKMARKS = "ShellBookmarksRegion";
        public const string SHELL_NEW = "ShellNewRegion";
        public const string SHELL_DIALOG = "ShellDialogRegion";
    }

    public static class SchemeNames
    {
        public const string SPLIT = "split";
        public const string SIGNATURE = "signature";

        public static class Dialog
        {
            public const string MESSAGE = "message";
            public const string PROGRESS = "progress";
        }
    }
}
