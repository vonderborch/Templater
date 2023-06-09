namespace Templater.Core
{
    public class PrepareOptions
    {
        /// <summary>
        /// The directory
        /// </summary>
        public string Directory;

        /// <summary>
        /// The output directory
        /// </summary>
        public string OutputDirectory;

        /// <summary>
        /// The skip cleaning
        /// </summary>
        public bool SkipCleaning;

        /// <summary>
        /// The template settings
        /// </summary>
        public Template.Template TemplateSettings;
    }
}