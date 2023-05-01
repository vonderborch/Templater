using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Templater.Core
{
    public class Settings
    {
        public string GitWebPath;

        public string GitAccessToken;

        public List<string> TemplateRepositories;

        public string SettingsVersion = Constants.SettingsFileVersion;

        public Settings()
        {
            TemplateRepositories = new List<string>() { Constants.DefaultTemplateRepository };
            GitWebPath = "https://github.com/";
            GitAccessToken = string.Empty;
        }

        [JsonIgnore]
        public string SecuredAccessToken => GitAccessToken == string.Empty ? string.Empty : $"?access_token=****";

        [JsonIgnore]
        public string RepositoriesAsString => string.Join(",", TemplateRepositories);

        public void LoadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            var rawContents = File.ReadAllText(fileName);
            if (!string.IsNullOrWhiteSpace(rawContents))
            {
                var contents = JsonConvert.DeserializeObject<Settings>(rawContents);

                if (contents != null)
                {
                    GitWebPath = contents.GitWebPath;
                    GitAccessToken = contents.GitAccessToken;
                    TemplateRepositories = contents.TemplateRepositories;
                }
            }
        }

        public void SaveFile(string fileName)
        {
            var contents = JsonConvert.SerializeObject(this, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            File.WriteAllText(fileName, contents);
        }
    }
}
