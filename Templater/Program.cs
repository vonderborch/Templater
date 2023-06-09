using CommandLine;
using Templater.Core;
using Templater.Options;

namespace Templater
{
    internal class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            /* Example args:
             * args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.BASE", "-o", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES" };
             *        Prepares the specified template solution
             * 
             * args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.BASE", "-o", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES", "s" };
             *        Prepares the specified template solution but skips cleanup
             * 
             * args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.BASE", "-o", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES", "i" };
             *        What-if preparation of the specified template solution
             *        
             *        
             *        
             *        
             * args = new string[] { "update-templates" };
             *        Checks for template updates
             *        
             * args = new string[] { "update-templates", "-f" };
             *        Forces template updates
            */
            args = new string[] { "update-templates", "-f" };

            // Ask for configuration if it doesn't exist
            if (!Core.Templater.Instance.ValidateConfiguration())
            {
                Console.WriteLine("Creating settings file...");
                new Configure().Execute(new Configure());
            }
            // Update templates
            if ((DateTime.Now - Core.Templater.Instance.Settings.LastTemplatesUpdateCheck).TotalSeconds > Core.Templater.Instance.Settings.SecondsBetweenTemplateUpdateChecks)
            {
                new UpdateTemplates().Execute(new UpdateTemplates());
            }

            // parse command line arguments and execute the appropriate command
            var parseResults = Parser.Default.ParseArguments<Prepare, Generate, ListTemplates, Configure, UpdateTemplates, ReportIssue>(args);

            parseResults.MapResult(
                (Prepare opts) => new Prepare().Execute(opts),
                (Generate opts) => new Generate().Execute(opts),
                (ListTemplates opts) => new ListTemplates().Execute(opts),
                (Configure opts) => new Configure().Execute(opts),
                (ReportIssue opts) => new ReportIssue().Execute(opts),
                (UpdateTemplates opts) => new UpdateTemplates().Execute(opts),
                _ => MakeError()
                                  );
        }

        /// <summary>
        /// Makes the error.
        /// </summary>
        /// <returns></returns>
        public static string MakeError()
        {
            return "\0";
        }
    }
}