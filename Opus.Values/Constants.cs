namespace Opus.Values
{
    /// <summary>
    /// Various types in arrays, supported by this application.
    /// </summary>
    public static class SupportedTypes
    {
        /// <summary>
        /// Supported cultures as two-letter language codes.
        /// </summary>
        public static readonly string[] CULTURES = { "fi", "sv", "en" };
    }

    /// <summary>
    /// File paths and other file information for various application level files.
    /// </summary>
    public static class FilePaths
    {
        /// <summary>
        /// Extension for pdf-files.
        /// </summary>
        public const string PDF_EXTENSION = ".pdf";
        /// <summary>
        /// Extension for Opus configuration files.
        /// </summary>
        public const string CONFIG_EXTENSION = ".ops";
        /// <summary>
        /// Directory for storing Opus configuration files.
        /// </summary>
        public static readonly string CONFIG_DIRECTORY = AppContext.BaseDirectory;
        /// <summary>
        /// Directory for storing importable profile files.
        /// </summary>
        public static readonly string PROFILE_DIRECTORY = Path.Combine(
            AppContext.BaseDirectory,
            "ProfileImport"
        );
    }

    /// <summary>
    /// Names of various display regions in the app UI.
    /// </summary>
    public static class RegionNames
    {
        /// <summary>
        /// Main section containing all other sections.
        /// </summary>
        public const string SHELL_MAINSECTION = "ShellMainSection";
        /// <summary>
        /// Main dialog section containing all dialog content.
        /// </summary>
        public const string SHELL_DIALOG = "MainSectionDialogRegion";
        /// <summary>
        /// Dialog content region, displays actual content.
        /// </summary>
        public const string DIALOG_CONTENT = "DialogRegionContent";

        /// <summary>
        /// The content section in a single region scheme.
        /// </summary>
        public const string MAINSECTION_ONE = "MainSectionSingle";

        /// <summary>
        /// The options region in a dual region scheme.
        /// </summary>
        public const string MAINSECTION_TWO_OPTIONS = "MainSectionDualOptions";
        /// <summary>
        /// The action region in a dual region scheme.
        /// </summary>
        public const string MAINSECTION_TWO_ACTION = "MainSectionDualAction";

        /// <summary>
        /// The file region in a three region scheme.
        /// </summary>
        public const string MAINSECTION_THREE_FILE = "MainSectionFileRegion";
        /// <summary>
        /// The action region in a three region scheme.
        /// </summary>
        public const string MAINSECTION_THREE_ACTION = "MainSectionActionRegion";
        /// <summary>
        /// The options region in a three region scheme.
        /// </summary>
        public const string MAINSECTION_THREE_OPTIONS = "MainSectionOptionsRegion";

        /// <summary>
        /// The file region in a four region scheme.
        /// </summary>
        public const string MAINSECTION_FOUR_FILE = "MainSectionFourFile";
        /// <summary>
        /// The action region in a four region scheme.
        /// </summary>
        public const string MAINSECTION_FOUR_ACTION = "MainSectionFourAction";
        /// <summary>
        /// The options region in a four region scheme.
        /// </summary>
        public const string MAINSECTION_FOUR_OPTIONS = "MainSectionFourOptions";
        /// <summary>
        /// The supporting action region in a four region scheme.
        /// </summary>
        public const string MAINSECTION_FOUR_SUPPORT = "MainSectionFourSupport";
    }

    /// <summary>
    /// Names of the navigation schemes used.
    /// </summary>
    public static class SchemeNames
    {
        /// <summary>
        /// Extract scheme.
        /// </summary>
        public const string EXTRACT = "extract";
        /// <summary>
        /// Work copy scheme.
        /// </summary>
        public const string WORKCOPY = "workcopy";
        /// <summary>
        /// Merge scheme.
        /// </summary>
        public const string MERGE = "merge";
        /// <summary>
        /// Compose scheme.
        /// </summary>
        public const string COMPOSE = "compose";
    }
}
