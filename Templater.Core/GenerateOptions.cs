using Newtonsoft.Json;
using Templater.Core.Solution;

namespace Templater.Core
{
    public class GenerateOptions
    {
        [JsonIgnore]
        private Dictionary<string, string> _replacementText = new Dictionary<string, string>();

        public string SolutionConfigFile;

        public string SolutionName;

        public bool CleanSolutionConfigFile;

        public bool OverrideExistingDirectory;

        public string Directory;

        public SolutionSettings SolutionSettings;

        public Template.Template Template;

        public string[][] GetSpecialReplacementText => new string[][]
        {
            new string[] { "<CurrentUserName>", Environment.UserName },
            new string[] { "<ParentDir>", Path.GetFileName(Directory) },
            new string[] { "<SolutionName>", SolutionName },
        };

        public Dictionary<string, string> ReplacementsAndGuids
        {
            get
            {
                if (_replacementText.Count == 0)
                {
                    // add the guids...
                    var guidCount = Templater.Instance.TemplateGuidCounts[Template.Name];
                    for (var i = 1; i <= guidCount; i++)
                    {
                        var guid = Guid.NewGuid().ToString();
                        var searchTerm = $"GUID{i.ToString(Constants.GUID_PADDING)}";
                        _replacementText.Add(searchTerm, guid);
                    }

                    foreach (var text in GetSpecialReplacementText)
                    {
                        _replacementText.Add(text[0], text[1]);
                    }

                    // add other replacements...
                    foreach (var replacement in Template.Settings.ReplacementText)
                    {
                        var searchTerm = replacement.Item1;
                        var replacementText = replacement.Item2;
                        for (var i = 0; i < GetSpecialReplacementText.Length; i++)
                        {
                            replacementText = replacementText.Replace(GetSpecialReplacementText[i][0], GetSpecialReplacementText[i][1]);
                        }

                        _replacementText.Add(searchTerm, replacementText);
                    }
                }

                return _replacementText;
            }
        }

        public string UpdateTextWithReplacements(string value)
        {
            foreach (var pair in ReplacementsAndGuids)
            {
                value = value.Replace(pair.Key, pair.Value);
            }

            return value;
        }
    }
}