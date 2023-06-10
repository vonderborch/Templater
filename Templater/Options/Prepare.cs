using CommandLine;
using Newtonsoft.Json;
using Templater.Helpers;

namespace Templater.Options
{
    [Verb("prepare", HelpText = "Prepare a template")]
    internal class Prepare : AbstractOption
    {
        [Option('d', "directory", Required = true, HelpText = "The directory to prepare as a template")]
        public string Directory { get; set; }

        [Option('o', "output-directory", Required = true, HelpText = "The output directory to place the template into")]
        public string OutputDirectory { get; set; }

        [Option('t', "type", Required = false, Default = "auto", HelpText = "The type of the solution to prepare. Defaults to auto.")]
        public string SolutionType { get; set; }

        [Option('s', "skip-cleaning", Required = false, Default = false, HelpText = "If flag is provided, the working directory won't be deleted at the end of the prepare process.")]
        public bool SkipCleaning { get; set; }

        [Option('i', "what-if", Required = false, Default = false, HelpText = "If flag is provided, the template will not be prepared, but the user will be guided through all settings.")]
        public bool WhatIf { get; set; }

        /// <summary>
        /// Executes the prepare template steps with the specified options.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The result of the execution.
        /// </returns>
        /// <exception cref="System.ArgumentException">Valid arguments: {validShortNames}</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">No valid solution type found in directory! Supported solution types: {validNames}</exception>
        /// <exception cref="System.IO.InvalidDataException">Template info file is invalid!</exception>
        public override string Execute(AbstractOption option)
        {
            Console.WriteLine("Gathering template configuration info...");
            // the actual options we are dealing with
            var options = (Prepare)option;

            // Solution type
            var validShortNames = string.Join(", ", Core.Templater.Instance.TemplaterShortNames);
            validShortNames = $"auto, {validShortNames}";
            switch (options.SolutionType.ToLowerInvariant())
            {
                case "auto":
                    break;

                case "dotsln":
                    break;

                default:

                    throw new ArgumentException($"Valid arguments: {validShortNames}");
            }
            options.SolutionType = DetectSolutionType(options.Directory);
            if (options.SolutionType == string.Empty)
            {
                var validNames = string.Join(", ", Core.Templater.Instance.TemplaterLongNames);
                throw new System.IO.DirectoryNotFoundException($"No valid solution type found in directory! Supported solution types: {validNames}");
            }

            // the prepare options
            var prepareOptions = new Core.PrepareOptions()
            {
                Directory = options.Directory,
                OutputDirectory = options.OutputDirectory,
                SkipCleaning = options.SkipCleaning,
            };

            // check if the solution directory already has a template info file...
            var templateInfoFileName = System.IO.Path.Combine(prepareOptions.Directory, Core.Constants.TemplaterTemplatesInfoFileName);
            if (System.IO.File.Exists(templateInfoFileName))
            {
                // ... if so, load it
                var contents = File.ReadAllText(templateInfoFileName);
                var templateInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Core.Template.Template>(contents);
                if (templateInfo == null)
                {
                    throw new System.IO.InvalidDataException("Template info file is invalid!");
                }
                else
                {
                    prepareOptions.TemplateSettings = templateInfo;
                }
            }

            // display settings and ask for confirmation
            prepareOptions.TemplateSettings = SetTemplateOptions(prepareOptions.TemplateSettings);
            var saveFileContents = JsonConvert.SerializeObject(prepareOptions.TemplateSettings, Formatting.Indented);
            File.WriteAllText(templateInfoFileName, saveFileContents);

            // try to prepare the solution
            if (!options.WhatIf)
            {
                Console.WriteLine($"Preparing solution {prepareOptions.Directory} as {options.SolutionType}...");
                var result = Core.Templater.Instance.Prepare(prepareOptions, options.SolutionType);
                Console.WriteLine(result);
                return result;
            }
            // Otherwise, the user will be told that their settings have been saved and the project not prepared
            else
            {
                Console.WriteLine($"Template not prepared, but configuration settings saved: {templateInfoFileName}");
                return "Success";
            }
        }

        public Templater.Core.Template.Template SetTemplateOptions(Templater.Core.Template.Template template)
        {
            // if we have existing config, ask if it is fine...
            if (template != null)
            {
                var firstSettings = JsonConvert.SerializeObject(template, Formatting.Indented);
                var result = ConsoleHelpers.GetYesNo($"Do the settings look correct?{Environment.NewLine}{firstSettings}{Environment.NewLine}");
                if (result)
                {
                    return template;
                }
            }

            // defaults
            if (template == null)
            {
                template = new Core.Template.Template();
            }
            if (template.Settings == null)
            {
                template.Settings = new Core.Template.TemplateSettings();
            }
            if (template.Settings.NugetSettings == null)
            {
                template.Settings.NugetSettings = new Core.Template.NugetSettings();
            }

            var settings = string.Empty;
            do
            {
                // Go through each main option and ask about it
                template.Name = ConsoleHelpers.GetInput($"Template Name", template.Name ?? string.Empty);
                template.Description = ConsoleHelpers.GetInput($"Template Description", template.Description ?? string.Empty);
                template.Author = ConsoleHelpers.GetInput($"Template Author", template.Author ?? string.Empty);
                template.Version = ConsoleHelpers.GetInput($"Template Description", template.Version ?? string.Empty);

                // go through each settings option and ask about it
                Console.WriteLine("Special Text: <CurrentFullName>, <ParentDir>, <ProjectName>");
                template.Settings.DefaultAuthor = ConsoleHelpers.GetInput($"Default Author", template.Settings.DefaultAuthor ?? string.Empty);
                template.Settings.DefaultCompanyName = ConsoleHelpers.GetInput($"Default Company", template.Settings.DefaultCompanyName ?? string.Empty);
                template.Settings.DefaultSolutionName = ConsoleHelpers.GetInput($"Default Solution Name", template.Settings.DefaultSolutionName ?? string.Empty);
                template.Settings.DefaultSolutionNameFormat = ConsoleHelpers.GetInput($"Default Solution Name Format", template.Settings.DefaultSolutionNameFormat ?? string.Empty);
                template.Settings.DefaultDescription = ConsoleHelpers.GetInput($"Default Description", template.Settings.DefaultDescription ?? string.Empty);
                template.Settings.RenameOnlyFilesAndDirectories = ConsoleHelpers.GetInput("Rename-only Files and Directories (comma-separated)", string.Join(",", template.Settings.RenameOnlyFilesAndDirectories) ?? string.Empty).Split(",").ToList();

                var replacementTextExisting = template.Settings.ReplacementText == null ? string.Empty : string.Join(",", template.Settings.ReplacementText.Select(x => $"({x.Item1}, {x.Item2})"));
                while (ConsoleHelpers.GetYesNo($"Update replacement text? (Existing: {replacementTextExisting})", false))
                {
                    var newReplacementText = new List<Tuple<string, string>>();
                    // Update existing
                    if (template.Settings.ReplacementText != null)
                    {
                        foreach (var pair in template.Settings.ReplacementText)
                        {
                            var printedPair = $"({pair.Item1}, {pair.Item2})";
                            if (ConsoleHelpers.GetYesNo($"Delete pair {printedPair}?", false))
                            {
                                continue;
                            }
                            else
                            {
                                var newPair = ConsoleHelpers.GetInput("New replacement text pair (key = item 1, value = item 2. Items comma-separated)", printedPair);

                                var split = newPair.Split(",");
                                if (split.Length > 2)
                                {
                                    throw new Exception("Only 1 comma allowed!");
                                }
                                newReplacementText.Add(new Tuple<string, string>(split[0], split[1]));
                            }
                        }
                    }

                    // add new
                    while (ConsoleHelpers.GetYesNo("Add new replacement text pair?", false))
                    {
                        var newPair = ConsoleHelpers.GetInput("New replacement text pair (key = item 1, value = item 2. Items comma-separated)");

                        var split = newPair.Split(",");
                        if (split.Length > 2)
                        {
                            throw new Exception("Only 1 comma allowed!");
                        }
                        newReplacementText.Add(new Tuple<string, string>(split[0], split[1]));
                    }

                    template.Settings.ReplacementText = newReplacementText;
                    replacementTextExisting = template.Settings.ReplacementText == null ? string.Empty : string.Join(",", template.Settings.ReplacementText.Select(x => $"({x.Item1}, {x.Item2})"));
                }

                template.Settings.Instructions = ConsoleHelpers.GetInput("Instructions (semi-colan-separated)", string.Join(";", template.Settings.Instructions) ?? string.Empty).Split(";").ToList();
                template.Settings.Commands = ConsoleHelpers.GetInput("Commands (semi-colan-separated)", string.Join(";", template.Settings.Commands) ?? string.Empty).Split(";").ToList();
                template.Settings.CleanupFilesAndDirectories = ConsoleHelpers.GetInput("Cleanup Files and Directories (comma-separated)", string.Join(",", template.Settings.CleanupFilesAndDirectories) ?? string.Empty).Split(",").ToList();
                template.Settings.DirectoriesExcludedInPrepare = ConsoleHelpers.GetInput("Prepare-excluded Directories (comma-separated)", string.Join(",", template.Settings.DirectoriesExcludedInPrepare) ?? string.Empty).Split(",").ToList();

                // Nuget settings
                template.Settings.NugetSettings.AskForNugetInfo = ConsoleHelpers.GetYesNo("Ask for nuget information?", true);
                template.Settings.NugetSettings.DefaultNugetLicense = ConsoleHelpers.GetInput("Nuget License", string.IsNullOrEmpty(template.Settings.NugetSettings.DefaultNugetLicense) ? "MIT" : template.Settings.NugetSettings.DefaultNugetLicense);
                template.Settings.NugetSettings.DefaultNugetTags = ConsoleHelpers.GetInput("Nuget Tags", template.Settings.NugetSettings.DefaultNugetTags);

                // serialize for display
                settings = JsonConvert.SerializeObject(template, Formatting.Indented);
            } while (!ConsoleHelpers.GetYesNo($"Do the settings look correct?{Environment.NewLine}{settings}{Environment.NewLine}"));

            return template;
        }

        /// <summary>
        /// Detects the type of the solution.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        private static string DetectSolutionType(string directory)
        {
            foreach (var templater in Templater.Core.Templater.Instance.TemplaterImplementations)
            {
                if (templater.DirectoryValidForTemplater(directory))
                {
                    return templater.ShortName;
                }
            }

            return "";
        }
    }
}