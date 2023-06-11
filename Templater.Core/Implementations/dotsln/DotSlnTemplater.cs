using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Templater.Core.Implementations.dotsln
{
    internal class DotSlnTemplater : AbstractTemplater
    {
        /// <summary>
        /// The regex tags
        /// </summary>
        private static readonly string[][] REGEX_TAGS =
        {
            new string[] { "Authors", Constants.REGEX_TAGS[0] }
            , new string[] { "Company", Constants.REGEX_TAGS[1] }
            , new string[] { "PackageTags", Constants.REGEX_TAGS[2] }
            , new string[] { "Description", Constants.REGEX_TAGS[3] }
            , new string[] { "PackageLicenseExpression", Constants.REGEX_TAGS[4] }
            , new string[] { "Version", Constants.REGEX_TAGS[5] }
            , new string[] { "FileVersion", Constants.REGEX_TAGS[5] }
            , new string[] { "AssemblyVersion", Constants.REGEX_TAGS[5] }
        };

        /// <summary>
        /// The files to update
        /// </summary>
        private static readonly string[] FILES_TO_UPDATE =
        {
            ".sln",
            ".csproj",
            ".shproj",
            ".projitems"
        };

        public DotSlnTemplater() : base("VisualStudio (.sln)", "dotsln")
        {
        }

        /// <summary>
        /// Checks whether the directory is valid for this templater.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns>
        /// True if valid, False otherwise.
        /// </returns>
        public override bool DirectoryValidForTemplater(string directory)
        {
            foreach (var file in System.IO.Directory.GetFiles(directory))
            {
                if (file.EndsWith(".sln"))
                {
                    return true;
                }
            }

            foreach (var subDirectory in System.IO.Directory.GetDirectories(directory))
            {
                var result = DirectoryValidForTemplater(subDirectory);
                if (result)
                {
                    return result;
                }
            }

            return false;
        }

        /// <summary>
        /// Prepares the template.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The preperation result.
        /// </returns>
        public override string Prepare(PrepareOptions options, Func<string, bool> log)
        {
            var startTime = DateTime.Now;
            var directoryName = options.TemplateSettings.Name.Replace(" ", "_");
            var actualDirectory = Path.Combine(options.OutputDirectory, directoryName);

            // Step 1 - Delete the working directory if it already exists
            log($"Checking if working directory '{actualDirectory}' exists...");
            if (Directory.Exists(actualDirectory))
            {
                log("  Deleting existing directory...");
                Directory.Delete(actualDirectory, true);
                log("  Directory deleted!");
            }

            // Step 2 - Copy the source directory to the working directory
            log($"Copying base solution '{options.Directory}' to working direcotry '{actualDirectory}'...");
            CopyDirectory(options.Directory, actualDirectory, options.TemplateSettings.Settings.DirectoriesExcludedInPrepare);
            log("  Base solution copied!");

            // Step 3 - Get Guids and Update template_info.json
            log("Getting GUIDs in solution...");
            var guids = GetGuids(actualDirectory);
            log($"  Found {guids.Count} GUIDs!");
            log($"  Updating template '{Constants.TemplaterTemplatesInfoFileName}' file with GUID count...");
            var templateInfo = JsonConvert.DeserializeObject<Template.TemplateWithGuids>(File.ReadAllText(Path.Combine(actualDirectory, Constants.TemplaterTemplatesInfoFileName)));
            templateInfo.GuidsCount = guids.Count;
            File.WriteAllText(Path.Combine(actualDirectory, Constants.TemplaterTemplatesInfoFileName), JsonConvert.SerializeObject(templateInfo, Formatting.Indented));
            log($"  Template '{Constants.TemplaterTemplatesInfoFileName}' file updated!");

            // Step 4 - Update any solutions in the working directory
            log("Updating template with generic replacement text...");
            UpdateSolutions(actualDirectory, options, guids);
            log("  Template updated!");

            // Step 5 - Archive the Directory and Delete the working directory
            log("Packaging template...");
            var archivePath = Path.Combine(options.OutputDirectory, $"{directoryName}.zip");
            ArchiveDirectory(actualDirectory, archivePath, options.SkipCleaning);
            log("  Template packaged!");

            log("Work complete!");
            var totalTime = DateTime.Now - startTime;
            return $"Successfully prepared the template in {totalTime.TotalSeconds.ToString("0.00")} second(s): {archivePath}";
        }

        /// <summary>
        /// Updates the solution in the working directory to be templated.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="options">The options.</param>
        private void UpdateSolutions(string directory, PrepareOptions options, Dictionary<string, string> guids)
        {
            // Step 4a - Create the replacement dictionaries...
            var slnReplacements = GetSolutionFileReplacements(guids, options);
            var otherReplacements = GetOtherFileReplacements(options);

            // Step 4b - Update the all files
            UpdateFiles(directory, slnReplacements, otherReplacements);
        }

        /// <summary>
        /// Gets the solution file replacements.
        /// </summary>
        /// <param name="guids">The guids.</param>
        /// <param name="options">The options.</param>
        /// <returns>The replacement dictionary for .sln files.</returns>
        private Dictionary<string, string> GetSolutionFileReplacements(Dictionary<string, string> guids, PrepareOptions options)
        {
            var slnReplacements = new Dictionary<string, string>(guids);
            foreach (var replacement in options.TemplateSettings.Settings.ReplacementText)
            {
                slnReplacements.Add(replacement.Item1, replacement.Item2);
            }
            return slnReplacements;
        }

        /// <summary>
        /// Gets the other file replacements.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The replacement dictionary for non-.sln files.</returns>
        private Dictionary<Regex, string> GetOtherFileReplacements(PrepareOptions options)
        {
            // Other file replacements
            Dictionary<Regex, string> otherReplacements = new();
            for (var i = 0; i < REGEX_TAGS.Length; i++)
            {
                otherReplacements.Add(new Regex($"<{REGEX_TAGS[i][0]}>.*<\\/{REGEX_TAGS[i][0]}>"), $"<{REGEX_TAGS[i][0]}>{REGEX_TAGS[i][1]}</{REGEX_TAGS[i][0]}>");
            }

            foreach (var replacement in options.TemplateSettings.Settings.ReplacementText)
            {
                otherReplacements.Add(new Regex(replacement.Item1), replacement.Item2);
            }

            return otherReplacements;
        }

        /// <summary>
        /// Gets the guids.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns>All guids for all .sln files in the directory we're templating.</returns>
        public Dictionary<string, string> GetGuids(string directory)
        {
            Dictionary<string, string> output = new();

            // try to find .sln files
            var files = Directory.GetFiles(directory);
            for (var i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]) == ".sln")
                {
                    var lines = File.ReadAllLines(files[i]);

                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Project(", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var splitByComma = line.Split(",");
                            var last = splitByComma[splitByComma.Length - 1];

                            var guid = last.Substring(3, last.Length - 5);
                            output.Add(guid, $"GUID{output.Count.ToString(Constants.GUID_PADDING)}");
                        }
                    }
                }
            }

            // look through sub-directories
            var directories = Directory.GetDirectories(directory);
            for (var i = 0; i < directories.Length; i++)
            {
                var directoryGuids = GetGuids(directories[i]);
                foreach (var guid in directoryGuids)
                {
                    output.Add(guid.Key, guid.Value);
                }
            }

            return output;
        }

        /// <summary>
        /// Updates the files.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="slnReplacements">The SLN replacements.</param>
        /// <param name="otherReplacements">The other replacements.</param>
        private void UpdateFiles(string directory, Dictionary<string, string> slnReplacements, Dictionary<Regex, string> otherReplacements)
        {
            var files = Directory.GetFiles(directory).Where(f => FILES_TO_UPDATE.Contains(Path.GetExtension(f)) && Path.GetFileName(f) != Constants.TemplaterTemplatesInfoFileName).ToList();

            for (var i = 0; i < files.Count; i++)
            {
                var fileContents = File.ReadAllLines(files[i]);
                // if it is a .sln file...
                if (Path.GetExtension(files[i]) == ".sln")
                {
                    File.WriteAllText(files[i], UpdateSolutionFile(fileContents, slnReplacements));
                }
                // otherwise...
                else
                {
                    File.WriteAllText(files[i], UpdateOtherFile(fileContents, otherReplacements));
                }
            }

            // Update csproj files in sub-directories
            var directories = Directory.GetDirectories(directory);
            for (var i = 0; i < directories.Length; i++)
            {
                UpdateFiles(directories[i], slnReplacements, otherReplacements);
            }
        }

        /// <summary>
        /// Updates the solution file.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="replacements">The replacements.</param>
        /// <returns>The updted file contents.</returns>
        private string UpdateSolutionFile(string[] fileContents, Dictionary<string, string> replacements)
        {
            var mergedContents = string.Join(Environment.NewLine, fileContents);

            foreach (var pair in replacements)
            {
                mergedContents = mergedContents.Replace(pair.Key, pair.Value);
            }

            return mergedContents;
        }

        /// <summary>
        /// Updates the other file.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="replacements">The replacements.</param>
        /// <returns>The updated file contents.</returns>
        private string UpdateOtherFile(string[] fileContents, Dictionary<Regex, string> replacements)
        {
            var output = new StringBuilder();

            for (var i = 0; i < fileContents.Length; i++)
            {
                var textLine = fileContents[i];
                foreach (var replacement in replacements)
                {
                    textLine = replacement.Key.Replace(textLine, replacement.Value);
                }

                output.AppendLine(textLine);
            }

            return output.ToString();
        }
    }
}