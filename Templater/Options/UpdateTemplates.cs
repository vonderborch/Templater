using CommandLine;

namespace Templater.Options
{
    [Verb("update-templates", HelpText = "Checks for updated templates")]
    internal class UpdateTemplates : AbstractOption
    {
        public override string Execute(AbstractOption option)
        {
            throw new System.NotImplementedException();
        }
    }
}
