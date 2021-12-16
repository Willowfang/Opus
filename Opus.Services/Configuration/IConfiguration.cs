using Opus.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Configuration
{
    /// <summary>
    /// Static storage class for configuration services
    /// </summary>
    public static class IConfiguration
    {
        /// <summary>
        /// Provides functionality for application-wide effects
        /// </summary>
        public interface App
        {
            /// <summary>
            /// Change language preference
            /// </summary>
            /// <param name="ISO639_1">Language to assign</param>
            public void ChangeLanguage(string ISO639_1);
            /// <summary>
            /// Return current language code in ISO639-1 format
            /// </summary>
            /// <returns></returns>
            public string GetLanguage();
        }
        /// <summary>
        /// Provides funtionality for signature-related tasks
        /// </summary>
        public interface Sign
        {
            /// <summary>
            /// Postfix for unsigned files
            /// </summary>
            public string SignatureRemovePostfix { get; set; }
        }
        /// <summary>
        /// Provides functionality for tasks related to file merging
        /// </summary>
        public interface Merge
        {
            /// <summary>
            /// If true, page numbers will be added to a merged document 
            /// </summary>
            public bool AddPageNumbers { get; set; }
        }
        /// <summary>
        /// Provides functionality for tasks related to composing a pdf-file
        /// from separate files
        /// </summary>
        public interface Compose
        {
            /// <summary>
            /// Get all saved composition profiles
            /// </summary>
            /// <returns></returns>
            public IList<ICompositionProfile> GetProfiles();
            /// <summary>
            /// Save a new profile
            /// </summary>
            /// <param name="profile">Profile to save</param>
            /// <returns>Saved profile</returns>
            public ICompositionProfile SaveProfile(ICompositionProfile profile);
        }
    }
}
