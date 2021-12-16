using CX.PdfLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Core.ExtensionMethods
{
    public static class EnumerableExtensions
    {
        public static string GetResourceString(this ProgressPhase phase)
        {
            return phase switch
            {
                ProgressPhase.Unassigned => Resources.PhaseNames.Unassigned,
                ProgressPhase.AddingBookmarks => Resources.PhaseNames.AddingBookmarks,
                ProgressPhase.AddingPageNumbers => Resources.PhaseNames.AddingPageNumbers,
                ProgressPhase.Converting => Resources.PhaseNames.Converting,
                ProgressPhase.Extracting => Resources.PhaseNames.Extracting,
                ProgressPhase.GettingBookmarks => Resources.PhaseNames.GettingBookmarks,
                ProgressPhase.Merging => Resources.PhaseNames.Merging,
                ProgressPhase.Finished => Resources.PhaseNames.Finished,
                _ => throw new ArgumentOutOfRangeException("Phase not defined")
            };
        }
    }
}
