using Newtonsoft.Json.Converters;

namespace Templater.Core.Solution
{
    public class GitSettings
    {
        /// <summary>
        /// The repo name
        /// </summary>
        public string RepoName;

        /// <summary>
        /// The repo owner
        /// </summary>
        public string RepoOwner;

        /// <summary>
        /// The repo mode
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public GitRepoMode RepoMode = GitRepoMode.NoRepo;

        /// <summary>
        /// The is private
        /// </summary>
        public bool IsPrivate = true;
    }
}