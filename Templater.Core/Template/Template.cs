using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;
using Templater.Core.Repositories;

namespace Templater.Core.Template
{
    public class Template
    {
        /// <summary>
        /// The name
        /// </summary>
        public string Name;

        /// <summary>
        /// The description
        /// </summary>
        public string Description;

        /// <summary>
        /// The author
        /// </summary>
        public string Author;

        /// <summary>
        /// The version
        /// </summary>
        public string Version;

        /// <summary>
        /// The information on the repo for the template
        /// </summary>
        [JsonIgnore]
        public TemplateInfo RepoInfo;

        /// <summary>
        /// The settings
        /// </summary>
        public TemplateSettings Settings;

        /// <summary>
        /// The file path
        /// </summary>
        [JsonIgnore]
        public string FilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Template"/> class.
        /// </summary>
        public Template()
        {
            Name = null;
            Description = null;
            Author = null;
            Version = null;
            RepoInfo = null;
            Settings = null;
            FilePath = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Template"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="repoInfo">The repo information.</param>
        public Template(string file, TemplateInfo repoInfo)
        {
            FilePath = file;
            RepoInfo = repoInfo;

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

                            JsonConvert.PopulateObject(contents, this);
                        }
                    }
                }
            }

            if (Settings == null)
            {
                throw new Exception($"Template {Name} is invalid!");
            }
            Settings?.ReplacementText.Add(new Tuple<string, string>(Path.GetFileNameWithoutExtension(file), Constants.SpecialTextProjectName));
            Settings?.ReplacementText.Add(new Tuple<string, string>(Name, Constants.SpecialTextProjectName));
        }
    }
}