using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Opus.Values
{
    public static class SupportedTypes
    {
        public static readonly string[] CULTURES = { "fi", "sv", "en" };
    }

    public static class FilePaths
    {
        public const string PDF_EXTENSION = ".pdf";
        public const string CONFIG_EXTENSION = ".ops";
        public static readonly string CONFIG_DIRECTORY = AppContext.BaseDirectory;
        public static readonly string PROFILE_DIRECTORY = Path.Combine(AppContext.BaseDirectory, "ProfileImport");
        public static readonly string LOCALUPDATEINFOLOCATION = Path.Combine(AppContext.BaseDirectory, "UpdateInfo.json");
        public static readonly string TEST_LOCALUPDATEINFOLOC = @"C:\Users\Public\UpdateInfo.json";
        public static readonly string SETUPFILENAME = "Opus.msi";
        public const string UPDATEINFONAME = "UpdateInfo.json";
    }

    public static class RegionNames
    {
        public const string SHELL_MAINSECTION = "ShellMainSection";
        public const string SHELL_DIALOG = "MainSectionDialogRegion";
        public const string DIALOG_CONTENT = "DialogRegionContent";

        public const string MAINSECTION_ONE = "MainSectionSingle";

        public const string MAINSECTION_TWO_OPTIONS = "MainSectionDualOptions";
        public const string MAINSECTION_TWO_ACTION = "MainSectionDualAction";

        public const string MAINSECTION_THREE_FILE = "MainSectionFileRegion";
        public const string MAINSECTION_THREE_ACTION = "MainSectionActionRegion";
        public const string MAINSECTION_THREE_OPTIONS = "MainSectionOptionsRegion";

        public const string MAINSECTION_FOUR_FILE = "MainSectionFourFile";
        public const string MAINSECTION_FOUR_ACTION = "MainSectionFourAction";
        public const string MAINSECTION_FOUR_OPTIONS = "MainSectionFourOptions";
        public const string MAINSECTION_FOUR_SUPPORT = "MainSectionFourSupport";
    }

    public static class SchemeNames
    {
        public const string SPLIT = "split";
        public const string WORKCOPY = "workcopy";
        public const string MERGE = "merge";
        public const string COMPOSE = "compose";
    }
}
