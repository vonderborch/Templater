using Newtonsoft.Json;

namespace Templater.Core.Repositories
{
    public class TemplateInfoList
    {
        /// <summary>
        /// The templates
        /// </summary>
        public List<TemplateInfo> Templates = new();

        /// <summary>
        /// Gets the template map.
        /// </summary>
        /// <value>
        /// The template map.
        /// </value>
        [JsonIgnore]
        public Dictionary<string, TemplateInfo> TemplateMap => Templates.ToDictionary(t => t.Name, t => t);
    }
}