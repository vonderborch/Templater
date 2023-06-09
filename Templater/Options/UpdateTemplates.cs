using CommandLine;

namespace Templater.Options
{
    [Verb("update-templates", HelpText = "Checks for updated templates")]
    internal class UpdateTemplates : AbstractOption
    {
        [Option('f', "force", Required = false, Default = false, HelpText = "If flag is provided, the program will force all templates to redownload.")]
        public bool ForceUpdate { get; set; }

        /// <summary>
        /// Executes the update templates steps with the specified options.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The result of the execution.
        /// </returns>
        public override string Execute(AbstractOption option)
        {
            var options = (UpdateTemplates)option;

            var startTime = DateTime.Now;
            Console.WriteLine("Updating templates...");
            var counts = Core.Templater.Instance.UpdateTemplates(options.ForceUpdate);
            var totalTime = DateTime.Now - startTime;
            var totalSeconds = totalTime.TotalSeconds.ToString("0.00");
            Console.WriteLine($"Template updating/downloading ran in {totalSeconds} second(s). Results:");

            var localCount = counts.Item1;
            var remoteCount = counts.Item2;
            var newCount = counts.Item3;
            var updatedCount = counts.Item4;

            if (localCount > remoteCount)
            {
                Console.WriteLine($"Orphaned templates (only exist locally): {localCount - remoteCount}");
            }

            if (newCount + updatedCount == 0)
            {
                Console.WriteLine($"No new or updated templates");
            }
            if (newCount > 0)
            {
                Console.WriteLine($"Downloaded {newCount} new template(s)");
            }
            if (updatedCount > 0)
            {
                Console.WriteLine($"Updated {updatedCount} template(s)");
            }

            return "Templates updated";
        }
    }
}