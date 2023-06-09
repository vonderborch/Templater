using CommandLine;

namespace Templater.Options
{
    [Verb("report-issue", HelpText = "Report an issue")]
    internal class ReportIssue : AbstractOption
    {
        /// <summary>
        /// Executes the report issue steps with the specified options.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The result of the execution.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string Execute(AbstractOption option)
        {
            throw new System.NotImplementedException();
        }
    }
}