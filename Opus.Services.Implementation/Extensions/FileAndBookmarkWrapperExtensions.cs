﻿using Opus.Services.Implementation.Data.Extraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="FileAndBookmarkWrapper"/>s.
    /// </summary>
    public static class FileAndBookmarkWrapperExtensions
    {
        /// <summary>
        /// Return only those wrappers, that have some extractable content (discard i.e. placeholders).
        /// </summary>
        /// <param name="original">All wrappers.</param>
        /// <returns>Wrappers with extractable content.</returns>
        public static List<FileAndBookmarkWrapper> Extractables(
            this IEnumerable<FileAndBookmarkWrapper> original
        )
        {
            return original.Where(x => x.Bookmark.Pages.Count >= 1).ToList();
        }
    }
}
