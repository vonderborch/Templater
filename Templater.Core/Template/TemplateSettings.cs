namespace Templater.Core.Template
{
    public class TemplateSettings
    {
        /// <summary>
        /// The default author for a new solution using this template
        /// </summary>
        public string DefaultAuthor = string.Empty;

        /// <summary>
        /// The default company name for a new solution using this template
        /// </summary>
        public string DefaultCompanyName = string.Empty;

        /// <summary>
        /// The default solution name for a new solution using this template. If this is set, DefaultSolutionNameFormat is ignored.
        /// </summary>
        public string DefaultSolutionName = string.Empty;

        /// <summary>
        /// The default solution name format for a new solution using this template
        /// </summary>
        public string DefaultSolutionNameFormat = string.Empty;

        /// <summary>
        /// The default description for a new solution using this template
        /// </summary>
        public string DefaultDescription = string.Empty;

        /// <summary>
        /// The nuget settings for a new solution using this template
        /// </summary>
        public NugetSettings NugetSettings = new();

        /// <summary>
        /// The directories excluded in prepare
        /// </summary>
        public List<string> DirectoriesExcludedInPrepare = new();

        /// <summary>
        /// Files and directories we only rename if need, not edit the contents of, when creating a new solution using this template
        /// </summary>
        public List<string> RenameOnlyFilesAndDirectories = new();

        /// <summary>
        /// Text to replace in the solution's files and directories after a new solution using this template has been created
        /// </summary>
        public List<Tuple<string, string>> ReplacementText = new();

        /// <summary>
        /// The commands to run after a new solution using this template has been created
        /// </summary>
        public List<string> Commands = new();

        /// <summary>
        /// Manual instructions to display after a new solution using this template has been created
        /// </summary>
        public List<string> Instructions = new();

        /// <summary>
        /// The files and directories to remove from a new solution using this template
        /// </summary>
        public List<string> CleanupFilesAndDirectories = new();
    }
}