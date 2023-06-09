using Templater.Core.Solution;

namespace Templater.Core
{
    public class GenerateOptions
    {
        public string SolutionConfigFile;

        public bool CleanSolutionConfigFile;

        public string Directory;

        public SolutionSettings SolutionSettings;

        public Template.Template Template;
    }
}