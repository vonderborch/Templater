using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Octokit;
using System.Text;
using System.Text.RegularExpressions;
using Templater.Core.Implementations.dotsln;
using Templater.Core.Repositories;
using Templater.Core.Template;
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
        public static Templater Instance { get { return lazy.Value; } }

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
        /// Prepares the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Type {type} not implemented!</exception>
        public string Prepare(PrepareOptions options, string type)
        {
            if (TemplaterMap.TryGetValue(type, out var templater))
            {
                return templater.Prepare(options);
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
                    Directory.Delete(TemplatesDirectory, true );
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