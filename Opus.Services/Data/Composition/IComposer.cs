using Opus.Services.Data.Composition;
using System.Threading.Tasks;

namespace Opus.Services.Data
{
    /// <summary>
    /// Service interface for composing a document.
    /// </summary>
    public interface IComposer
    {
        /// <summary>
        /// Compose a pdf document.
        /// </summary>
        /// <param name="directory">Directory to perform the composition from.</param>
        /// <param name="compositionProfile">Profile the composition is based on.</param>
        /// <param name="deleteConverted">If true, delete files that have been temporarily converted to pdf.</param>
        /// <param name="searchSubDirectories">If true, search for profile-defined files also from subfolders.</param>
        /// <returns></returns>
        public Task Compose(
            string directory,
            ICompositionProfile compositionProfile,
            bool deleteConverted,
            bool searchSubDirectories
        );
    }
}
