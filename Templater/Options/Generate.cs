using CommandLine;

namespace Templater.Options
{
    [Verb("generate", HelpText = "Generate a project from a template")]
    internal class Generate : AbstractOption
    {
        /// <summary>
        /// Executes solution generation with the specified options.
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