using CommandLine;
using Newtonsoft.Json;
using Templater.Core;
using Templater.Core.Solution;
using Templater.Helpers;

namespace Templater.Options
{
    [Verb("generate", HelpText = "Generate a project from a template")]
    internal class Generate : AbstractOption
    {
        [Option('n', "name", Required = true, HelpText = "The name for the generated solution")]
        public string Name { get; set; }

        [Option('t', "template", Required = true, HelpText = "The template to use")]
        public string Template { get; set; }

        [Option('o', "output-directory", Required = true, HelpText = "The output directory for the new solution")]
        public string OutputDirectory { get; set; }

        [Option('c', "solution-config", Required = false, HelpText = "The specific solution config file to use.")]
        public string SolutionConfig { get; set; } = "";

        [Option('f', "force", Required = false, Default = false, HelpText = "Overrides the existing directory if it already exists.")]
        public bool Force { get; set; }

        [Option('i', "what-if", Required = false, Default = false, HelpText = "If flag is provided, the solution will not be generated, but the user will be guided through all settings.")]
        public bool WhatIf { get; set; }

        /// <summary>
        /// Executes solution generation with the specified options.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The result of the execution.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string Execute(AbstractOption option)
        {
            Console.WriteLine("Validating selected template...");
            var options = (Generate)option;

            // validate and get the selected template
            Core.Templater.Instance.RefreshLocalTemplatesList();
            var exists = Core.Templater.Instance.TemplatesMap.TryGetValue(options.Template, out var template);

            if (!exists)
            {
                throw new Exception($"Template '{options.Template}' is not a valid template!");
            }

            Console.WriteLine($"Gathering solution configuration for new solution '{options.Name}'...");

            // determine if we need to load an existing config file...
            string solutionConfig = "";
            var cleanupSolutionConfigFile = true;
            if (!string.IsNullOrEmpty(options.SolutionConfig))
            {
                solutionConfig = options.SolutionConfig;
                cleanupSolutionConfigFile = false;
            }
            else
            {
                solutionConfig = Core.Templater.Instance.TemplaterSolutionConfigurationFile;
            }

            // load or initialize
            var generateOptions = new GenerateOptions()
            {
                SolutionName = options.Name,
                SolutionConfigFile = solutionConfig,
                CleanSolutionConfigFile = cleanupSolutionConfigFile,
                Directory = options.OutputDirectory,
                OverrideExistingDirectory = options.Force,
                SolutionSettings = null,
                Template = template
            };

            if (File.Exists(solutionConfig))
            {
                Console.WriteLine($"Loading solution config file: {solutionConfig}");
                try
                {
                    generateOptions.SolutionSettings = JsonConvert.DeserializeObject<SolutionSettings>(File.ReadAllText(solutionConfig));
                }
                catch
                {
                    Console.WriteLine($"Unable to load config file: {solutionConfig}");
                    generateOptions.SolutionSettings = null;
                }
            }

            // Display settings and ask for confirmation
            SetSolutionConfiguration(generateOptions);

            // Save the solution config for later use or documentation if needed
            var saveFileContents = JsonConvert.SerializeObject(generateOptions.SolutionSettings, Formatting.Indented);
            File.WriteAllText(generateOptions.SolutionConfigFile, saveFileContents);

            if (!options.WhatIf)
            {
                // generate solution...
                Console.WriteLine($"Generating solution '{generateOptions.SolutionName}' using template '{generateOptions.Template.Name}'...");
                var result = Core.Templater.Instance.Generate(generateOptions, LogMessage, LogMessage, DoNothing);
                Console.WriteLine(result);
                return result;
            }
            else
            {
                // done!
                Console.WriteLine($"Solution not generated, but configuration settings saved: {generateOptions.SolutionConfigFile}");

                return "Success";
            }
        }

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private bool LogMessage(string value)
        {
            Console.WriteLine(value);
            return true;
        }

        /// <summary>
        /// Do Nothing.
        /// </summary>
        /// <param name="_">The .</param>
        /// <returns></returns>
        private bool DoNothing(string _)
        {
            return true;
        }

        /// <summary>
        /// Sets the solution configuration.
        /// </summary>
        /// <param name="options">The options.</param>
        private void SetSolutionConfiguration(GenerateOptions options)
        {
            var solutionSettings = options.SolutionSettings;
            // if we have existing config, ask if it is fine...
            if (solutionSettings != null)
            {
                var firstSettings = JsonConvert.SerializeObject(solutionSettings, Formatting.Indented);
                var result = ConsoleHelpers.GetYesNo($"Do the settings look correct?{Environment.NewLine}{firstSettings}{Environment.NewLine}");
                if (result)
                {
                    return;
                }
            }

            // defaults
            if (solutionSettings == null)
            {
                solutionSettings = new SolutionSettings();
            }
            if (solutionSettings.GitSettings == null)
            {
                solutionSettings.GitSettings = new GitSettings();
            }

            // go through options and ask about them
            var settings = string.Empty;
            do
            {
                // go through each main option and ask about it
                solutionSettings.Author = ConsoleHelpers.GetInput($"Solution Author", solutionSettings.Author ?? options.UpdateTextWithReplacements(options.Template.Settings.DefaultAuthor));

                solutionSettings.Description = ConsoleHelpers.GetInput($"Solution Description", solutionSettings.Description ?? options.UpdateTextWithReplacements(options.Template.Settings.DefaultDescription));

                solutionSettings.Version = ConsoleHelpers.GetInput($"Solution Starting Version", solutionSettings.Version ?? "1.0.0");

                // nuget settings
                var tags = "";
                if (solutionSettings.Tags != null)
                {
                    tags = string.Join(",", solutionSettings.Tags);
                }
                else
                {
                    tags = options.Template.Settings.NugetSettings.DefaultNugetTags;
                }
                tags = options.UpdateTextWithReplacements(tags);
                solutionSettings.Tags = ConsoleHelpers.GetInput("Solution Tags (comma-separated)", string.Join(",", tags)).Split(",").ToArray();
                solutionSettings.Tags = solutionSettings.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                solutionSettings.LicenseExpresion = ConsoleHelpers.GetInput($"Solution LicenseExpresion", solutionSettings.LicenseExpresion ?? options.Template.Settings.NugetSettings.DefaultNugetLicense);

                // go through each git option and ask about it
                solutionSettings.GitSettings.RepoMode = ConsoleHelpers.GetInputForEnum<GitRepoMode>($"Git Repo Mode", solutionSettings.GitSettings.RepoMode.ToString());
                if (solutionSettings.GitSettings.RepoMode == GitRepoMode.NewRepoFull)
                {
                    solutionSettings.GitSettings.RepoOwner = ConsoleHelpers.GetInput($"Git Repo Owner", solutionSettings.GitSettings.RepoOwner ?? string.Empty);

                    solutionSettings.GitSettings.RepoName = ConsoleHelpers.GetInput($"Git Repo Name", solutionSettings.GitSettings.RepoName ?? Name.Replace(" ", "-")).Replace(" ", "-");

                    solutionSettings.GitSettings.IsPrivate = ConsoleHelpers.GetYesNo("Git Repo Private", solutionSettings.GitSettings.IsPrivate);
                }

                // serialize for display
                settings = JsonConvert.SerializeObject(solutionSettings, Formatting.Indented);
            } while (!ConsoleHelpers.GetYesNo($"Do the settings look correct?{Environment.NewLine}{settings}{Environment.NewLine}"));

            options.SolutionSettings = solutionSettings;
        }
    }
}