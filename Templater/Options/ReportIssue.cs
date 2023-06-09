using CommandLine;
using Templater.Helpers;

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
        public override string Execute(AbstractOption option)
        {
            Console.WriteLine("Opening browser to https://github.com/vonderborch/Templater/issues/new ...");
            UrlHelpers.OpenUrl("https://github.com/vonderborch/Templater/issues/new", "Please go to https://github.com/vonderborch/Templater/issues/new to file a bug or request a new feature!");

            return "Success";
        }
    }
}