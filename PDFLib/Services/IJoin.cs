using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFLib.Services
{
    public interface IJoin
    {
        public void Combine(string[] filePaths, string outputPath);
    }
}
