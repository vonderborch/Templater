using CommandLine;
using Templater.Core;
using Templater.Helpers;

namespace Templater.Options
{
    [Verb("configure", HelpText = "Configure settings")]
    internal class Configure : AbstractOption
    {
        /// <summary>
        /// Executes the configuration steps with the specified options.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The result of the execution.
        /// </returns>
        public override string Execute(AbstractOption option)
        {
            var settings = new Settings();
            if (File.Exists(Core.Templater.Instance.SettingsFileName))
            {
                settings.LoadFile(Core.Templater.Instance.SettingsFileName);
            }

            settings.GitWebPath = ConsoleHelpers.GetInput("Git URL", settings.GitWebPath);

            settings.GitAccessToken = ConsoleHelpers.GetInput("Git Personal Access Token", settings.GitAccessToken, settings.SecuredAccessToken);

            var repos = !string.IsNullOrEmpty(settings.RepositoriesAsString) ? settings.RepositoriesAsString : Constants.DefaultTemplateRepository;
            var repositories = ConsoleHelpers.GetInput("Template repositories (comma separated)", repos);

            settings.TemplateRepositories = repositories.Split(",").ToList();

            settings.SaveFile(Core.Templater.Instance.SettingsFileName);
            return "Settings saved!";
        }
    }
}