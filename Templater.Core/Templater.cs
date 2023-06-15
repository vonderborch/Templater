using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Octokit;
using System.Diagnostics;
using System.Text;
using Templater.Core.Implementations.dotsln;
using Templater.Core.Repositories;
using FileMode = System.IO.FileMode;

namespace Templater.Core
{
    public sealed class Templater
    {
        /// <summary>
        /// The lazy
        /// </summary>
        private static readonly Lazy<Templater> lazy = new Lazy<Templater>(() => new Templater());

        /// <summary>
        /// The settings
        /// </summary>
        private Settings? _settings;

        /// <summary>
        /// The git client
        /// </summary>
        private GitHubClient? _gitClient;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static Templater Instance
        { get { return lazy.Value; } }

        /// <summary>
        /// The templates
        /// </summary>
        public List<Template.Template> Templates = new();

        /// <summary>
        /// The template unique identifier counts
        /// </summary>
        public Dictionary<string, int> TemplateGuidCounts = new();

        /// <summary>
        /// Gets the templates map.
        /// </summary>
        /// <value>
        /// The templates map.
        /// </value>
        public Dictionary<string, Template.Template> TemplatesMap => Templates.ToDictionary(t => t.Name, t => t);

        /// <summary>
        /// Prevents a default instance of the <see cref="Templater"/> class from being created.
        /// </summary>
        private Templater()
        {
            _settings = null;
            TemplaterImplementations = new()
            {
                new DotSlnTemplater()
            };
        }

        /// <summary>
        /// Gets the templater implementations.
        /// </summary>
        /// <value>
        /// The templater implementations.
        /// </value>
        public List<AbstractTemplater> TemplaterImplementations { get; }

        /// <summary>
        /// Gets the templater map.
        /// </summary>
        /// <value>
        /// The templater map.
        /// </value>
        public Dictionary<string, AbstractTemplater> TemplaterMap => TemplaterImplementations.ToDictionary(t => t.ShortName, t => t);

        /// <summary>
        /// Gets the templater short names.
        /// </summary>
        /// <value>
        /// The templater short names.
        /// </value>
        public List<string> TemplaterShortNames => TemplaterImplementations.Select(t => t.ShortName).ToList();

        /// <summary>
        /// Gets the templater long names.
        /// </summary>
        /// <value>
        /// The templater long names.
        /// </value>
        public List<string> TemplaterLongNames => TemplaterImplementations.Select(t => t.LongName).ToList();

        /// <summary>
        /// Gets the name of the settings file.
        /// </summary>
        /// <value>
        /// The name of the settings file.
        /// </value>
        public string SettingsFileName => Path.Combine(CoreDirectory, Constants.TemplaterSettingsFileName);

        /// <summary>
        /// Gets the templater solution configuration file.
        /// </summary>
        /// <value>
        /// The templater solution configuration file.
        /// </value>
        public string TemplaterSolutionConfigurationFile => Path.Combine(CoreDirectory, Constants.TemplaterSolutionConfigFileName);

        /// <summary>
        /// The templater solution configuration backup directory
        /// </summary>
        public string TemplaterSolutionConfigurationBackupDirectory => Path.Combine(CoreDirectory, Constants.TemplaterSolutionConfigBackupDirectory);

        /// <summary>
        /// Gets the templates directory.
        /// </summary>
        /// <value>
        /// The templates directory.
        /// </value>
        public string TemplatesDirectory => Path.Combine(CoreDirectory, Constants.TemplaterTemplatesDirectory);

        /// <summary>
        /// Gets the name of the templates information file.
        /// </summary>
        /// <value>
        /// The name of the templates information file.
        /// </value>
        public string TemplatesCacheFileName => Path.Combine(CoreDirectory, Constants.TemplaterTemplatesCacheFileName);

        /// <summary>
        /// Gets the core directory.
        /// </summary>
        /// <value>
        /// The core directory.
        /// </value>
        public string CoreDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Constants.TemplaterDirectory);

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public Settings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new Settings();
                    _settings.LoadFile(SettingsFileName);
                }
                return _settings;
            }
        }

        /// <summary>
        /// Gets the git client.
        /// </summary>
        /// <value>
        /// The git client.
        /// </value>
        public GitHubClient GitClient
        {
            get
            {
                if (_gitClient == null)
                {
                    _gitClient = new GitHubClient(new ProductHeaderValue(Constants.AppName), new Uri(Settings.GitWebPath));
                    var tokenAuth = new Credentials(Settings.GitAccessToken);
                    _gitClient.Credentials = tokenAuth;
                }
                return _gitClient;
            }
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <returns></returns>
        public bool ValidateConfiguration()
        {
            return File.Exists(SettingsFileName);
        }

        /// <summary>
        /// Generates the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Type {type} not implemented!</exception>
        public string Generate(GenerateOptions options, Func<string, bool> log, Func<string, bool> instructionLog, Func<string, bool> commandLog)
        {
            var startTime = DateTime.Now;
            var directoryName = options.SolutionName;
            var actualDirectory = Path.Combine(options.Directory, directoryName);

            // Step 1 - Check if the directory exists and raise an error if it does. Or delete if we're allowed to
            log($"Checking if the new solution directory '{actualDirectory}' already exists...");
            if (Directory.Exists(actualDirectory))
            {
                if (options.OverrideExistingDirectory)
                {
                    log("  Deleting existing directory...");
                    Directory.Delete(actualDirectory, true);
                    log("  Existing directory deleted!");
                }
                else
                {
                    throw new Exception($"Directory {actualDirectory} already exists. Please delete it or use the override option.");
                }
            }

            // Step 2 - Git
            if (options.SolutionSettings.GitSettings.RepoMode != Solution.GitRepoMode.NoRepo)
            {
                var gitUrl = $"https://github.com/{options.SolutionSettings.GitSettings.RepoOwner}/{options.SolutionSettings.GitSettings.RepoName}.git";
                switch (options.SolutionSettings.GitSettings.RepoMode)
                {
                    case Solution.GitRepoMode.NewRepoOnlyInit:
                        log($"Initializing Git Repo at destination directory '{actualDirectory}' ...");
                        Directory.CreateDirectory(actualDirectory);
                        LibGit2Sharp.Repository.Init(actualDirectory);
                        log("  Directory initialized!");
                        break;

                    case Solution.GitRepoMode.NewRepoFull:
                        log($"Creating git repo: {gitUrl} ...");
                        var repository = new NewRepository(options.SolutionSettings.GitSettings.RepoName)
                        {
                            AutoInit = false,
                            Description = options.SolutionSettings.Description,
                            LicenseTemplate = options.SolutionSettings.LicenseExpresion,
                            Private = options.SolutionSettings.GitSettings.IsPrivate,
                        };

                        var context = Templater.Instance.GitClient.Repository.Create(repository);
                        log("  Git repo created! Cloning repo...");
                        LibGit2Sharp.Repository.Clone(gitUrl, Path.GetDirectoryName(actualDirectory));
                        actualDirectory = Path.Combine(options.Directory, options.SolutionSettings.GitSettings.RepoName);
                        log($"  Repo cloned to '{actualDirectory}'");
                        break;
                }
            }
            else
            {
                log($"Creating destination directory '{actualDirectory}' ...");
                Directory.CreateDirectory(actualDirectory);
                log("  Directory created!");
            }

            // Step 3 - Unzip the template
            log($"Unzipping template '{options.Template.Name}' ...");
            UnzipTemplate(actualDirectory, options.Template);
            log("  Template unzipped!");

            // Step 4 - Update the files
            log("Updating template files for solution...");
            options.UpdateReplacementTextWithTags();
            UpdateFiles(actualDirectory, options);
            log("  Files updated!!");

            // Step 5 - Run commands (if possible)
            if (options.Template.Settings.Commands.Count == 0)
            {
                log("No commands to run, skipping step!");
            }
            else
            {
                log("Running commands...");
                var cmdStart = $"/C cd \"{actualDirectory}\"";
                var i = 0;
                try
                {
                    for (i = 0; i < options.Template.Settings.Commands.Count; i++)
                    {
                        log($"  Running command {i + 1}/{options.Template.Settings.Commands.Count}...");
                        commandLog("Executing command {i + 1}/{.Template.Settings.Commands.Count}: {options.Template.Settings.Commands[i]}");
                        RunCommand(actualDirectory, options.Template.Settings.Commands[i]);
                    }
                    log("  All commands completed!");
                }
                catch (Exception ex)
                {
                    log($"  Failed running command {i + 1}, skipping remaining commands. Please execute the below when manually:");
                    if (log != instructionLog)
                    {
                        instructionLog($"Failed running command {i + 1}, skipping remaining commands. Please execute the below when manually:");
                    }
                    for (_ = i; i < options.Template.Settings.Commands.Count; i++)
                    {
                        log($"    {cmdStart} & {options.Template.Settings.Commands[i]}");
                        instructionLog($"  {cmdStart} & {options.Template.Settings.Commands[i]}");
                    }
                }
            }

            // Step 6 - Cleanup after ourselves
            if (options.Template.Settings.CleanupFilesAndDirectories.Count == 0)
            {
                log("Nothing to cleanup, skipping step!");
            }
            else
            {
                log("Cleaning up...");
                var filesDeleted = 0;
                var directoriesDeleted = 0;
                for (var i = 0; i < options.Template.Settings.CleanupFilesAndDirectories.Count; i++)
                {
                    if (Directory.Exists(options.Template.Settings.CleanupFilesAndDirectories[i]))
                    {
                        directoriesDeleted++;
                        Directory.Delete(options.Template.Settings.CleanupFilesAndDirectories[i], true);
                    }
                    else
                    {
                        filesDeleted++;
                        File.Delete(options.Template.Settings.CleanupFilesAndDirectories[i]);
                    }
                }
                log($"  Cleaned up '{filesDeleted}' file(s) and '{directoriesDeleted}' directories!");
            }

            // Step 7 - Display Instructions
            if (options.Template.Settings.Instructions.Count == 0)
            {
                log("No instructions, skipping step!");
            }
            else
            {
                log("Printing instructions...");
                var instructions = new StringBuilder();
                for (var i = 0; i < options.Template.Settings.Instructions.Count; i++)
                {
                    instructions.AppendLine(options.Template.Settings.Instructions[i]);
                }
                instructionLog(instructions.ToString());
                log("  Instructions printed!");
            }

            // Step 8 - Cleanup the solution_config.json file
            if (!options.CleanSolutionConfigFile)
            {
                log($"Skipping clean up of {Constants.TemplaterSolutionConfigFileName}");
            }
            else
            {
                log($"Cleaning up {Constants.TemplaterSolutionConfigFileName}...");
                if (!Directory.Exists(TemplaterSolutionConfigurationBackupDirectory))
                {
                    Directory.CreateDirectory(TemplaterSolutionConfigurationBackupDirectory);
                }

                string backupFile = string.Empty;
                do
                {
                    var backupFileName = $"{options.SolutionName.Replace(" ", "-")}_{startTime.ToString("yyyyMMddHHmmss")}.json";
                    backupFile = Path.Combine(TemplaterSolutionConfigurationBackupDirectory, backupFileName);
                } while (File.Exists(backupFile));
                File.Move(options.SolutionConfigFile, backupFile);
                log($"  Solution config file backed up to: {backupFile}");
            }

            // Done!
            log("Work complete!");
            var totalTime = DateTime.Now - startTime;
            return $"Successfully prepared created the solution in {totalTime.TotalSeconds.ToString("0.00")} second(s): {actualDirectory}";
        }

        /// <summary>
        /// Unzips the template.
        /// </summary>
        /// <param name="outputDirectory">The output directory.</param>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        private void UnzipTemplate(string outputDirectory, Template.Template template)
        {
            using (var file = File.OpenRead(template.FilePath))
            {
                using (var zip = new ZipFile(file))
                {
                    foreach (ZipEntry entry in zip)
                    {
                        var path = Path.Combine(outputDirectory, entry.Name.Replace("/", Path.DirectorySeparatorChar.ToString()));
                        var directoryPath = Path.GetDirectoryName(path);
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            if (entry.IsDirectory)
                            {
                                Directory.CreateDirectory(path);
                            }
                            else
                            {
                                if (Constants.ExcludedGenerateFiles.Contains(Path.GetFileName(entry.Name)))
                                {
                                    continue;
                                }

                                if (directoryPath is { Length: > 0 })
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                var buffer = new byte[4096];
                                if (File.Exists(path))
                                {
                                    File.Delete(path);
                                }

                                using (var inputStream = zip.GetInputStream(entry))
                                {
                                    using (var output = File.Create(path))
                                    {
                                        StreamUtils.Copy(inputStream, output, buffer);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RunCommand(string directory, string command)
        {
            if (OperatingSystem.IsWindows())
            {
                var cmdStart = $"/C cd \"{actualDirectory}\"";
                var cmd = Process.Start(Constants.CommandPrompt, $"{cmdStart} & {options.Template.Settings.Commands[i]}");
                cmd.WaitForExit();
            }
            else if (OperatingSystem.IsMacOS())
            {
                // TODO
                throw new NotImplementedException("Command execution not supported on this OS!");
            }
            else if (OperatingSystem.IsLinux())
            {
                // TODO
                throw new NotImplementedException("Command execution not supported on this OS!");
            }
            else
            {
                throw new NotImplementedException("Command execution not supported on this OS!");
            }
        }

        /// <summary>
        /// Updates the files.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        private void UpdateFiles(string directory, GenerateOptions options)
        {
            var entries = Directory.GetFileSystemEntries(directory);
            foreach (var entry in entries)
            {
                // skip the .git directory
                if (entry == ".git")
                {
                    continue;
                }

                var newEntryPath = options.UpdateTextWithReplacements(entry);

                // Update directory naming
                if (Directory.Exists(entry))
                {
                    var path = entry;
                    if (entry != newEntryPath && !options.Template.Settings.RenameOnlyFilesAndDirectories.Contains(Path.GetFileName(entry)))
                    {
                        Directory.Move(entry, newEntryPath);
                        path = newEntryPath;
                    }

                    // update inner files...
                    UpdateFiles(path, options);
                }
                // update files as needed
                else
                {
                    if (entry != newEntryPath)
                    {
                        if (File.Exists(newEntryPath))
                        {
                            File.Delete(newEntryPath);
                        }

                        File.Move(entry, newEntryPath);
                    }

                    if (!options.Template.Settings.RenameOnlyFilesAndDirectories.Contains(Path.GetFileName(newEntryPath)))
                    {
                        var text = File.ReadAllText(newEntryPath);
                        text = options.UpdateTextWithReplacements(text);
                        File.WriteAllText(newEntryPath, text);
                    }
                }
            }
        }

        /// <summary>
        /// Prepares the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Type {type} not implemented!</exception>
        public string Prepare(PrepareOptions options, string type, Func<string, bool> log)
        {
            if (TemplaterMap.TryGetValue(type, out var templater))
            {
                return templater.Prepare(options, log);
            }

            throw new NotImplementedException($"Type {type} not implemented!");
        }

        /// <summary>
        /// Updates the templates.
        /// </summary>
        public (int, int, int, int, List<string>) UpdateTemplates(bool forceUpdate = false)
        {
            var remoteTemplates = new TemplateInfoList();

            // Delete the templates directory and filecache if force update is true
            if (forceUpdate)
            {
                if (Directory.Exists(TemplatesDirectory))
                {
                    Directory.Delete(TemplatesDirectory, true);
                }
                if (File.Exists(TemplatesCacheFileName))
                {
                    File.Delete(TemplatesCacheFileName);
                }
            }

            // Create the templates directory and filecache if it doesn't exist
            if (!Directory.Exists(TemplatesDirectory))
            {
                Directory.CreateDirectory(TemplatesDirectory);
            }
            if (!File.Exists(TemplatesCacheFileName))
            {
                File.WriteAllText(TemplatesCacheFileName, JsonConvert.SerializeObject(remoteTemplates, Formatting.Indented));
            }

            // Get all templates from all repositories
            foreach (var repository in Settings.UniqueRepositories)
            {
                var repoTemplates = GetTemplateListForRepository(repository);
                remoteTemplates.Templates.AddRange(repoTemplates);
            }

            // load the local templatesinfo file to see current template status
            var localTemplates = JsonConvert.DeserializeObject<TemplateInfoList>(File.ReadAllText(TemplatesCacheFileName));
            if (localTemplates == null)
            {
                localTemplates = new TemplateInfoList();
            }

            // compare the two lists to see what needs to be updated
            var templatesToUpdate = remoteTemplates.Templates.Where(t => !localTemplates.TemplateMap.ContainsKey(t.Name) || localTemplates.TemplateMap[t.Name].SHA != t.SHA).ToList();

            // download the new/updated templates
            var totalNew = 0;
            var totalUpdated = 0;
            foreach (var template in templatesToUpdate)
            {
                if (template != null)
                {
                    var file = Path.Combine(TemplatesDirectory, template.Name);
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }

                    using (var client = new HttpClient())
                    {
                        using (var s = client.GetStreamAsync(template.Url))
                        {
                            using (var fs = new FileStream(file, FileMode.Create))
                            {
                                s.Result.CopyTo(fs);
                            }
                        }
                    }

                    // update the local templatesinfo mapping
                    if (localTemplates.TemplateMap.ContainsKey(template.Name))
                    {
                        foreach (var oldTemplate in localTemplates.Templates)
                        {
                            if (oldTemplate.Name == template.Name)
                            {
                                oldTemplate.SHA = template.SHA;
                                totalUpdated++;
                                break;
                            }
                        }
                    }
                    else
                    {
                        localTemplates.Templates.Add(template);
                        totalNew++;
                    }
                }
            }

            /// save the local templatesinfo file
            File.WriteAllText(TemplatesCacheFileName, JsonConvert.SerializeObject(localTemplates, Formatting.Indented));
            Settings.LastTemplatesUpdateCheck = DateTime.Now;
            Settings.SaveFile(SettingsFileName);

            // Load the templates
            RefreshLocalTemplatesList();

            // find orphaned templates and return info
            var orphanedTemplates = localTemplates.Templates.Where(t => !remoteTemplates.TemplateMap.ContainsKey(t.Name)).Select(t => t.Name).ToList();
            if (orphanedTemplates == null)
            {
                orphanedTemplates = new List<string>();
            }

            return (localTemplates.Templates.Count, remoteTemplates.Templates.Count, totalNew, totalUpdated, orphanedTemplates);
        }

        public void RefreshLocalTemplatesList()
        {
            // load the local templatesinfo file to see current template status
            var localTemplates = JsonConvert.DeserializeObject<TemplateInfoList>(File.ReadAllText(TemplatesCacheFileName));
            if (localTemplates == null)
            {
                localTemplates = new TemplateInfoList();
            }
            Templates.Clear();

            // create the templates directory if it doesn't exist
            if (!Directory.Exists(TemplatesDirectory))
            {
                Directory.CreateDirectory(TemplatesDirectory);
            }

            // go through each template in the templates directory...
            var templates = Directory.GetFiles(TemplatesDirectory, $"*.{Constants.TemplateFileType}");
            if (templates == null)
            {
                return;
            }

            foreach (var file in templates)
            {
                var fileName = Path.GetFileName(file);
                var repoMeta = localTemplates.TemplateMap.ContainsKey(fileName) ? localTemplates.TemplateMap[fileName] : null;
                var template = new Template.Template(file, repoMeta);
                TemplateGuidCounts[template.Name] = GetGuidCount(file);

                Templates.Add(template);
            }
        }

        private static int GetGuidCount(string file)
        {
            var count = -1;
            using (var fileStream = File.OpenRead(file))
            {
                using (var zip = new ZipFile(fileStream))
                {
                    foreach (ZipEntry entry in zip)
                    {
                        if (Path.GetFileName(entry.Name) == Constants.TemplaterTemplatesInfoFileName)
                        {
                            var contents = "";
                            using (var inputStream = zip.GetInputStream(entry))
                            {
                                using (var output = new MemoryStream())
                                {
                                    var buffer = new byte[4096];
                                    StreamUtils.Copy(inputStream, output, buffer);
                                    contents = Encoding.UTF8.GetString(output.ToArray());
                                }
                            }

                            var templateWithGuid = JsonConvert.DeserializeObject<Template.TemplateWithGuids>(contents);
                            count = templateWithGuid.GuidsCount;
                            break;
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Gets the template list for repository.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        public List<Repositories.TemplateInfo> GetTemplateListForRepository(string repository)
        {
            var splitName = repository.Split('/');
            var owner = splitName[splitName.Length - 2];
            var name = splitName[splitName.Length - 1];
            var rootContents = GitClient.Repository.Content.GetAllContents(owner, name).Result.ToList();

            var repoContents = rootContents.Select(x => new GitRepoContents(x, owner, name, x.Path)).ToList();

            var templates = GetTemplateInfo(repository, repoContents);
            return templates;
        }

        /// <summary>
        /// Gets the template information.
        /// </summary>
        /// <param name="repo">The repo.</param>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public List<Repositories.TemplateInfo> GetTemplateInfo(string repo, List<GitRepoContents> contents)
        {
            var output = new List<Repositories.TemplateInfo>();

            for (int i = 0; i < contents.Count; i++)
            {
                var content = contents[i];
                if (content.Info.Type == ContentType.Dir && content.ChildContent.Count > 0)
                {
                    output.AddRange(GetTemplateInfo(repo, content.ChildContent));
                }
                else if (content.Info.Name.EndsWith(Constants.TemplateFileType))
                {
                    var templateInfo = new Repositories.TemplateInfo(content.Info, repo);
                    output.Add(templateInfo);
                }
            }

            return output;
        }
    }
}