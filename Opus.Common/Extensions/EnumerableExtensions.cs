using WF.PdfLib.Common;

namespace Opus.Common.Extensions
{
    /// <summary>
    /// Extension methods for enumerables.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Get the correct resource string matching a progress phase.
        /// </summary>
        /// <param name="phase">Phase to match for.</param>
        /// <returns>The string representation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws, if no string for phase is found.</exception>
        public static string GetResourceString(this ProgressPhase phase)
        {
            return phase switch
            {
                ProgressPhase.Unassigned => Resources.Operations.PhaseNames.Unassigned,
                ProgressPhase.AddingBookmarks => Resources.Operations.PhaseNames.AddingBookmarks,
                ProgressPhase.AddingPageNumbers
                    => Resources.Operations.PhaseNames.AddingPageNumbers,
                ProgressPhase.Converting => Resources.Operations.PhaseNames.Converting,
                ProgressPhase.Extracting => Resources.Operations.PhaseNames.Extracting,
                ProgressPhase.GettingBookmarks => Resources.Operations.PhaseNames.GettingBookmarks,
                ProgressPhase.Merging => Resources.Operations.PhaseNames.Merging,
                ProgressPhase.Finished => Resources.Operations.PhaseNames.Finished,
                ProgressPhase.ChoosingDestination
                    => Resources.Operations.PhaseNames.ChoosingDestination,
                _ => throw new ArgumentOutOfRangeException("Phase not defined")
            };
        }
    }
}
