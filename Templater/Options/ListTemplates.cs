using CommandLine;

namespace Templater.Options
{
    [Verb("list-templates", HelpText = "List all available templates")]
    internal class ListTemplates : AbstractOption
    {
        /// <summary>
        /// Executes the list templates steps with the specified options.
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