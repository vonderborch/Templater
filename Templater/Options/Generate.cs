using CommandLine;

namespace Templater.Options
{
    [Verb("generate", HelpText = "Generate a project from a template")]
    internal class Generate : AbstractOption
    {
        public override string Execute(AbstractOption option)
        {
            throw new System.NotImplementedException();
        }
    }
}
