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
            
        }
    }
}
