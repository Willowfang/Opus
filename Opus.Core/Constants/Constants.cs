using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Opus.Core.Constants
{
    public static class ServiceNames
    {
        public const string LANGUAGEPROVIDER = "LanguageProvider";
    }

    public static class FilePaths
    {
        public const string CONFIG_EXTENSION = ".ops";
        public static string CONFIG_DIRECTORY = AppContext.BaseDirectory;
    }

    public static class RegionNames
    {
        public const string SHELL_MAINSECTION = "ShellMainSection";
        public const string SHELL_DIALOG = "MainSectionDialogRegion";
        public const string DIALOG_CONTENT = "DialogRegionContent";

        public const string MAINSECTION_ONE = "MainSectionSingle";

        public const string MAINSECTION_THREE_FILE = "MainSectionFileRegion";
        public const string MAINSECTION_THREE_ACTION = "MainSectionActionRegion";
        public const string MAINSECTION_THREE_OPTIONS = "MainSectionOptionsRegion";
    }

    public static class SchemeNames
    {
        public const string SPLIT = "split";
        public const string SIGNATURE = "signature";
        public const string JOIN = "join";

        public static class Dialog
        {
            public const string MESSAGE = "message";
            public const string PROGRESS = "progress";
        }
    }
}
