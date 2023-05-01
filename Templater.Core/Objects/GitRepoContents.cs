using Octokit;
using System.Diagnostics;

namespace Templater.Core.Objects
{
    [DebuggerDisplay("{Info}")]
    public class GitRepoContents
    {
        public RepositoryContent Info;
        public List<GitRepoContents> ChildContent;

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

        public override string ToString()
        {
            return Info.ToString() ?? string.Empty;
        }
    }
}
