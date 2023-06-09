namespace Templater.Core
{
    public static class Constants
    {
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
        public static string SpecialTextCurrentFullName = "<CurrentFullName>";

        /// <summary>
        /// Name of the specialtext project.
        /// </summary>
        public static string SpecialTextProjectName = "<ProjectName>";
    }
}