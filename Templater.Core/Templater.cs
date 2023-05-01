using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Templater.Core.Objects;

namespace Templater.Core
{
    public sealed class Templater
    {
        private static readonly Lazy<Templater> lazy =
    new Lazy<Templater>(() => new Templater());

        private Settings? _settings;

        private GitHubClient? _gitClient;

        public static Templater Instance { get { return lazy.Value; } }

        private Templater()
        {
            _settings = null;
        }

        public string SettingsFileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Constants.TemplaterDirectory, Constants.TemplaterSettingsFileName);

        public string TemplatesDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Constants.TemplaterDirectory, Constants.TemplaterTemplatesDirectory);

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

        public bool ValidateConfiguration()
        {
            return File.Exists(SettingsFileName);
        }

        public void UpdateTemplates()
        {
            // Get all templates from all repositories
            var allTemplates = new TemplateInfoList();
            foreach (var repository in Settings.TemplateRepositories)
            {
                var repoTemplates = GetTemplateListForRepository(repository);
                allTemplates.Templates.AddRange(repoTemplates);
            }

            // load the local templatesinfo file to see current template status

            // compare the two lists to see what needs to be updated

            // download the new/updated templates
        }

        public List<TemplateInfo> GetTemplateListForRepository(string repository)
        {
            var splitName = repository.Split('/');
            var owner = splitName[splitName.Length - 2];
            var name = splitName[splitName.Length - 1];
            var rootContents = GitClient.Repository.Content.GetAllContents(owner, name).Result.ToList();

            var repoContents = rootContents.Select(x => new GitRepoContents(x, owner, name, x.Path)).ToList();

            var templates = GetTemplateInfo(repository, repoContents);
            return templates;
        }

        public List<TemplateInfo> GetTemplateInfo(string repo, List<GitRepoContents> contents)
        {
            var output = new List<TemplateInfo>();

            for (int i = 0; i < contents.Count; i++)
            {
                var content = contents[i];
                if (content.Info.Type == ContentType.Dir && content.ChildContent.Count > 0)
                {
                    output.AddRange(GetTemplateInfo(repo, content.ChildContent));
                }
                else if (content.Info.Name.EndsWith(Constants.TemplateFileType))
                {
                    var templateInfo = new TemplateInfo(content.Info, repo);
                    output.Add(templateInfo);
                }
            }

            return output;
        }
    }
}
