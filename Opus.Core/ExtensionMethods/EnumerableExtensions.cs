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
                ProgressPhase.Extracting => Resources.PhaseNames.Extracting,
                ProgressPhase.AddingBookmarks => Resources.PhaseNames.AddingBookmarks,
                ProgressPhase.Finished => Resources.PhaseNames.Finished,
                _ => throw new ArgumentOutOfRangeException("Phase not defined")
            };
        }
    }
}
