using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.UpdateInfoCreator
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public string[] Notes { get; set; }
        public string SetupFileDirectory { get; set; }
    }
}
