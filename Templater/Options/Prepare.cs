using CommandLine;

namespace Templater.Options
{
    [Verb("prepare", HelpText = "Prepare a template")]
    internal class Prepare : AbstractOption
    {
        [Option('d', "directory", Required = true, HelpText = "The directory to prepare as a template")]
        public string Directory { get; set; }

        [Option('o', "output-directory", Required = false, HelpText = "The output directory to place the template into")]
        public string OutputDirectory { get; set; }

        public override string Execute(AbstractOption option)
        {
            var options = (Prepare)option;



            return "Solution prepared!";
        }
    }
}
