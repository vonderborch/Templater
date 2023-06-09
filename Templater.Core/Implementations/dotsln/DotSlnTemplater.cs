using System.Text;
using System.Text.RegularExpressions;

namespace Templater.Core.Implementations.dotsln
{
    internal class DotSlnTemplater : AbstractTemplater
    {
        /// <summary>
        /// The unique identifier padding length
        /// </summary>
        private const int GUID_PADDING_LENGTH = 9;

        /// <summary>
        /// The unique identifier padding
        /// </summary>
        private static readonly string GUID_PADDING = $"D{GUID_PADDING_LENGTH}";

        /// <summary>
        /// The regex tags
        /// </summary>
        private static readonly string[][] REGEX_TAGS =
        {
            new string[] { "Authors", "Author" }
            , new string[] { "Company", "Company" }
            , new string[] { "PackageTags", "tags" }
            , new string[] { "Description", "Description" }
            , new string[] { "PackageLicenseExpression", "License" }
            , new string[] { "Version", "Version" }
            , new string[] { "FileVersion", "Version" }
            , new string[] { "AssemblyVersion", "Version" }
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
        /// Generates a solution from a template.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The generation result.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string Generate(GenerateOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prepares the template.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The preperation result.
        /// </returns>
        public override string Prepare(PrepareOptions options)
        {
            var startTime = DateTime.Now;
            var directoryName = options.TemplateSettings.Name.Replace(" ", "_");
            var actualDirectory = Path.Combine(options.OutputDirectory, directoryName);

            // Step 1: Delete the working directory if it already exists
            if (Directory.Exists(actualDirectory))
            {
                Directory.Delete(actualDirectory, true);
            }

            // Step 2: Copy the source directory to the working directory
            CopyDirectory(options.Directory, actualDirectory, options.TemplateSettings.Settings.DirectoriesExcludedInPrepare);

            // Step 3 - Update any solutions in the working directory
            UpdateSolutions(actualDirectory, options);

            // Step 4 - Archive the Directory and Delete the working directory
            var archivePath = Path.Combine(options.OutputDirectory, $"{directoryName}.zip");
            ArchiveDirectory(actualDirectory, archivePath, options.SkipCleaning);

            var totalTime = DateTime.Now - startTime;
            return $"Successfully prepared the template in {totalTime.TotalSeconds.ToString("0.00")} second(s): {archivePath}";
        }

        /// <summary>
        /// Updates the solution in the working directory to be templated.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="options">The options.</param>
        private void UpdateSolutions(string directory, PrepareOptions options)
        {
            // Step 3a - Get the guids from the .SLN files...
            var guids = GetGuids(directory);

            // Step 3b - Create the replacement dictionaries...
            var slnReplacements = GetSolutionFileReplacements(guids, options);
            var otherReplacements = GetOtherFileReplacements(options);

            // Step 3c - Update the all files
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
                otherReplacements.Add(new Regex($"<{REGEX_TAGS[i][0]}>.*<\\/{REGEX_TAGS[i][0]}>"), $"<{REGEX_TAGS[i][0]}>[{REGEX_TAGS[i][1].ToUpperInvariant()}]</{REGEX_TAGS[i][0]}>");
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
        private Dictionary<string, string> GetGuids(string directory)
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
                            output.Add(guid, $"GUID{output.Count.ToString(GUID_PADDING)}");
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
            var files = Directory.GetFiles(directory).Where(f => FILES_TO_UPDATE.Contains(Path.GetExtension(f))).ToList();

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