using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Templater.Core.Objects
{
    [DebuggerDisplay("{Name}")]
    public class TemplateInfo
    {
        public string Name;
        public string SHA;
        public string Url;
        public string Repo;

        public TemplateInfo(RepositoryContent info, string repo)
        {
            Name = info.Name;
            SHA = info.Sha;
            Url = info.DownloadUrl;
            Repo = repo;
        }
    }
}
