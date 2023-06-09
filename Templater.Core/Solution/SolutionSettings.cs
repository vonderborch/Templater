using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Templater.Core.Solution
{
    public class SolutionSettings
    {
        public string Name;
        public string Author;
        public string Description;
        public List<string> Tags;
        public string LicenseExpresion;
        public string Version;
        public GitSettings GitSettings;
    }
}
