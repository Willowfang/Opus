using Opus.Common.Services.Data.Composition;
using Opus.Values;
using System.IO;
using System.Windows;

namespace Opus.Initialize
{
    internal class ProfileUpdater
    {
        private readonly ICompositionOptions options;

        public ProfileUpdater(
            ICompositionOptions options)
        {
            this.options = options;
        }

        internal void CheckNewProfilesAndUpdate()
        {
            bool errorFlag = false;

            if (Directory.Exists(FilePaths.PROFILE_DIRECTORY) == false)
                return;

            // Find new profile files in the profiles directory.
            foreach (string filePath in Directory.GetFiles(FilePaths.PROFILE_DIRECTORY))
            {
                try
                {
                    ImportFileAndDelete(filePath);
                }
                catch
                {
                    errorFlag = true;
                }
            }

            // If there was an error when importing profiles, notify the user.
            if (errorFlag)
            {
                MessageBox.Show(
                    Resources.Messages.StartUp.ProfileUpdateFailed,
                    Resources.Labels.General.Error
                );
            }
        }

        private void ImportFileAndDelete(string path)
        {
            // Import each profile and override and old one, if such a profile exist.
            ICompositionProfile profile = options.ImportProfile(path);
            options.SaveProfile(profile);
            File.Delete(path);
        }
    }
}
