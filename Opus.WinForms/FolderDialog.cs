namespace Opus.WinForms
{
    public static class FolderDialog
    {
        public static string? Open(
            string description,
            bool useDescriptionForTitle,
            bool showNewFolderButton)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = description;
            dialog.UseDescriptionForTitle = useDescriptionForTitle;
            dialog.ShowNewFolderButton = showNewFolderButton;

            if (dialog.ShowDialog() == DialogResult.Cancel)
                return null;

            return dialog.SelectedPath;
        }
    }
}
