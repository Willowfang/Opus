using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Base
{
    /// <summary>
    /// An item that is selectable.
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// If true, item is selected.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
