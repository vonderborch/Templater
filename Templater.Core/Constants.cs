using Templater.Core.Solution;

namespace Templater.Core
{
    public static class Constants
    {
        /// <summary>
        /// The unique identifier padding length
        /// </summary>
        private const int GUID_PADDING_LENGTH = 9;

        /// <summary>
        /// The unique identifier padding
        /// </summary>
        public static readonly string GUID_PADDING = $"D{GUID_PADDING_LENGTH}";

        /// <summary>
        /// The regex tags
        /// NOTE: Keep in sync with GenerateOptions().UpdateReplacementTextWithTags()
        /// </summary>
        public static readonly string[] REGEX_TAGS =
        {
            "[AUTHOR]",
            "[COMPANY]",
            "[TAGS]",
            "[DESCRIPTION]",
            "[LICENSE]",
            "[VERSION]",
        };

        /// <summary>
        /// The templater directory
        /// </summary>
        public static string TemplaterDirectory = "Templater";

        /// <summary>
        /// The templater settings file name
        /// </summary>
        public static string TemplaterSettingsFileName = "settings.json";

        /// <summary>
        /// The templater templates information file name
        /// </summary>
        public static string TemplaterTemplatesInfoFileName = "template_info.json";

        /// <summary>
        /// The templater templates information file name
        /// </summary>
        public static string TemplaterTemplatesCacheFileName = "template_cache.json";

        /// <summary>
        /// The templater solution configuration file name
        /// </summary>
        public static string TemplaterSolutionConfigFileName = "solution_config.json";

        /// <summary>
        /// The templater solution configuration backup directory
        /// </summary>
        public static string TemplaterSolutionConfigBackupDirectory = "SolutionConfigBackups";

        /// <summary>
        /// The templater templates directory
        /// </summary>
        public static string TemplaterTemplatesDirectory = "Templates";

        /// <summary>
        /// The application name
        /// </summary>
        public static string AppName = "Templater";

        /// <summary>
        /// The default github URL
        /// </summary>
        public static string DefaultGithubUrl = "https://github.com/";

        /// <summary>
        /// The default template repository
        /// </summary>
        public static string DefaultTemplateRepository = "https://github.com/vonderborch/Templater-Templates";

        /// <summary>
        /// The default seconds between template update checks
        /// By default, check for updates once per day
        /// </summary>
        public static int DefaultSecondsBetweenTemplateUpdateChecks = 86400;

        /// <summary>
        /// The settings file version
        /// </summary>
        public static string SettingsFileVersion = "1.0";

        /// <summary>
        /// The maximum git repo template search depth
        /// </summary>
        public static int MaxGitRepoTemplateSearchDepth = 1;

        /// <summary>
        /// The template file type
        /// </summary>
        public static string TemplateFileType = "zip";

        /// <summary>
        /// The specialtext parent dir.
        /// </summary>
        public static string SpecialTextParentDir = "<ParentDir>";

        /// <summary>
        //  Name of the special text current full.
        /// </summary>
        public static string SpecialTextCurrentFullName = "<CurrentUserName>";

        /// <summary>
        /// Name of the specialtext project.
        /// </summary>
        public static string SpecialTextProjectName = "<SolutionName>";

        /// <summary>
        /// The default git repo mode
        /// </summary>
        public static GitRepoMode DefaultGitRepoMode = GitRepoMode.NoRepo;

        /// <summary>
        /// The command prompt
        /// </summary>
        public static string CommandPrompt = "CMD.exe";

        /// <summary>
        /// The excluded files
        /// </summary>
        public static readonly string[] ExcludedGenerateFiles = { Constants.TemplaterTemplatesInfoFileName };
    }
}