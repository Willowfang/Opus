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
                ProgressPhase.Unassigned => Resources.Operations.PhaseNames.Unassigned,
                ProgressPhase.AddingBookmarks => Resources.Operations.PhaseNames.AddingBookmarks,
                ProgressPhase.AddingPageNumbers => Resources.Operations.PhaseNames.AddingPageNumbers,
                ProgressPhase.Converting => Resources.Operations.PhaseNames.Converting,
                ProgressPhase.Extracting => Resources.Operations.PhaseNames.Extracting,
                ProgressPhase.GettingBookmarks => Resources.Operations.PhaseNames.GettingBookmarks,
                ProgressPhase.Merging => Resources.Operations.PhaseNames.Merging,
                ProgressPhase.Finished => Resources.Operations.PhaseNames.Finished,
                _ => throw new ArgumentOutOfRangeException("Phase not defined")
            };
        }
    }
}
