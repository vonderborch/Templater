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
        /// <exception cref="System.NotImplementedException"></exception>
        public override string Execute(AbstractOption option)
        {
            var startTime = DateTime.Now;
            Console.WriteLine("Updating templates...");
            Core.Templater.Instance.UpdateTemplates(ForceUpdate);
            var totalTime = DateTime.Now - startTime;
            Console.WriteLine($"Templates updated in {totalTime.TotalSeconds.ToString("0.00")} second(s)!");
            return "Templates updated";
        }
    }
}