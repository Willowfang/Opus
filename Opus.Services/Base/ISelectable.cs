using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Base
{
    public interface ISelectable
    {
        public bool IsSelected { get; set; }
    }
}
