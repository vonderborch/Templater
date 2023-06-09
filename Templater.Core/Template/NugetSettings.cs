namespace Templater.Core.Template
{
    public class NugetSettings
    {
        /// <summary>
        /// Whether the solution using this template creates any nuget packages
        /// </summary>
        public bool AskForNugetInfo = false;

        /// <summary>
        /// The default nuget license for a new solution using this template
        /// </summary>
        public string DefaultNugetLicense = string.Empty;

        /// <summary>
        /// The default nuget tags for a new solution using this template
        /// </summary>
        public string DefaultNugetTags = string.Empty;
    }
}