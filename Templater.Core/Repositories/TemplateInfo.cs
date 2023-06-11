using Octokit;
using System.Diagnostics;

namespace Templater.Core.Repositories
{
    [DebuggerDisplay("{Name}")]
    public class TemplateInfo
    {
        /// <summary>
        /// The name
        /// </summary>
        public string Name;

        /// <summary>
        /// The sha
        /// </summary>
        public string SHA;

        /// <summary>
        /// The URL
        /// </summary>
        public string Url;

        /// <summary>
        /// The repo
        /// </summary>
        public string Repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateInfo"/> class.
        /// </summary>
        public TemplateInfo()
        {
            Name = "";
            SHA = "";
            Url = "";
            Repo = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateInfo"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="repo">The repo.</param>
        public TemplateInfo(RepositoryContent info, string repo)
        {
            Name = info.Name;
            SHA = info.Sha;
            Url = info.DownloadUrl;
            Repo = repo;
        }
    }
}