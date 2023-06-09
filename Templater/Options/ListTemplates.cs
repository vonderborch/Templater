using CommandLine;
using System.Text;

namespace Templater.Options
{
    [Verb("list-templates", HelpText = "List all available templates")]
    internal class ListTemplates : AbstractOption
    {
        [Option('q', "quick-info", Required = false, Default = false, HelpText = "If flag is provided, the program will just list the template names and not details on the templates.")]
        public bool QuickInfo { get; set; }

        /// <summary>
        /// Executes the list templates steps with the specified options.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>
        /// The result of the execution.
        /// </returns>
        public override string Execute(AbstractOption option)
        {
            var options = (ListTemplates)option;
            Console.WriteLine("Gathering available templates...");
            Core.Templater.Instance.RefreshLocalTemplatesList();

            var templates = new StringBuilder();
            foreach (var template in Core.Templater.Instance.Templates)
            {
                templates.AppendLine($" - {template.Name}");
                if (!options.QuickInfo)
                {
                    templates.AppendLine($"     Author: {template.Author}");
                    templates.AppendLine($"     Description: {template.Description}");
                    templates.AppendLine($"     Version: {template.Version}");
                }
            }

            Console.WriteLine("Available Templates:");
            Console.WriteLine(templates.ToString());

            return "Success";
        }
    }
}