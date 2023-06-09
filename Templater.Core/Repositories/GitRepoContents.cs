using Octokit;
using System.Diagnostics;

namespace Templater.Core.Repositories
{
    [DebuggerDisplay("{Info}")]
    public class GitRepoContents
    {
        /// <summary>
        /// The information
        /// </summary>
        public RepositoryContent Info;

        /// <summary>
        /// The child content
        /// </summary>
        public List<GitRepoContents> ChildContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitRepoContents"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="currentDepth">The current depth.</param>
        public GitRepoContents(RepositoryContent info, string owner, string name, string path, int currentDepth = 0)
        {
            Info = info;

            if (info.Type == ContentType.Dir && currentDepth < Constants.MaxGitRepoTemplateSearchDepth)
            {
                var childCOntents = Templater.Instance.GitClient.Repository.Content.GetAllContents(owner, name, path).Result;

                ChildContent = childCOntents.Select(c => new GitRepoContents(c, owner, name, c.Path, currentDepth + 1)).ToList();
            }
            else
            {
                ChildContent = new List<GitRepoContents>();
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Info.ToString() ?? string.Empty;
        }
    }
}