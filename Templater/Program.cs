using CommandLine;
using Templater.Options;

namespace Templater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.BASE" };

            // Ask for configuration if it doesn't exist
            if (!Core.Templater.Instance.ValidateConfiguration())
            {
                new Configure().Execute(new Configure());
            }
            // Update templates
            Core.Templater.Instance.UpdateTemplates();

            // parse command line arguments and execute the appropriate command
            var parseResults = Parser.Default.ParseArguments<Prepare, Generate, ListTemplates, Configure, ReportIssue>(args);

            parseResults.MapResult(
                (Prepare opts) => new Prepare().Execute(opts),
                (Generate opts) => new Generate().Execute(opts),
                (ListTemplates opts) => new ListTemplates().Execute(opts),
                (Configure opts) => new Configure().Execute(opts),
                (ReportIssue opts) => new ReportIssue().Execute(opts),
                _ => MakeError()
            );
        }

        public static string MakeError()
        {
            return "\0";
        }
    }
}