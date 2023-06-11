using Newtonsoft.Json;

namespace Templater.Core
{
    public class Settings
    {
        /// <summary>
        /// The git web path
        /// </summary>
        public string GitWebPath;

        /// <summary>
        /// The git access token
        /// </summary>
        public string GitAccessToken;

        /// <summary>
        /// The template repositories
        /// </summary>
        public List<string> TemplateRepositories;

        /// <summary>
        /// The last templates update check
        /// </summary>
        public DateTime LastTemplatesUpdateCheck;

        /// <summary>
        /// The seconds between template update checks
        /// </summary>
        public int SecondsBetweenTemplateUpdateChecks;

        /// <summary>
        /// The settings version
        /// </summary>
        public string SettingsVersion = Constants.SettingsFileVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            LastTemplatesUpdateCheck = DateTime.MinValue;
            SecondsBetweenTemplateUpdateChecks = Constants.DefaultSecondsBetweenTemplateUpdateChecks;
            TemplateRepositories = new List<string>();
            GitWebPath = "https://github.com/";
            GitAccessToken = string.Empty;
        }

        /// <summary>
        /// Gets the secured access token.
        /// </summary>
        /// <value>
        /// The secured access token.
        /// </value>
        [JsonIgnore]
        public string SecuredAccessToken => GitAccessToken == string.Empty ? string.Empty : $"?access_token=****";

        /// <summary>
        /// Gets the repositories as string.
        /// </summary>
        /// <value>
        /// The repositories as string.
        /// </value>
        [JsonIgnore]
        public string RepositoriesAsString => string.Join(",", TemplateRepositories);

        /// <summary>
        /// Gets the unique repositories.
        /// </summary>
        /// <value>
        /// The unique repositories.
        /// </value>
        [JsonIgnore]
        public List<string> UniqueRepositories => TemplateRepositories.Distinct().ToList();

        /// <summary>
        /// Loads the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void LoadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            var rawContents = File.ReadAllText(fileName);
            if (!string.IsNullOrWhiteSpace(rawContents))
            {
                JsonConvert.PopulateObject(rawContents, this);
                SaveFile(fileName);
            }
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void SaveFile(string fileName)
        {
            var contents = JsonConvert.SerializeObject(this, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            File.WriteAllText(fileName, contents);
        }
    }
}