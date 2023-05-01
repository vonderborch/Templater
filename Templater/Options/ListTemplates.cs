using CommandLine;

namespace Templater.Options
{
    [Verb("list-templates", HelpText = "List all available templates")]
    internal class ListTemplates : AbstractOption
    {
        public override string Execute(AbstractOption option)
        {
            throw new System.NotImplementedException();
        }
    }
}
