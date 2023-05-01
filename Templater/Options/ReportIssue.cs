using CommandLine;

namespace Templater.Options
{
    [Verb("report-issue", HelpText = "Report an issue")]
    internal class ReportIssue : AbstractOption
    {
        public override string Execute(AbstractOption option)
        {
            throw new System.NotImplementedException();
        }
    }
}
