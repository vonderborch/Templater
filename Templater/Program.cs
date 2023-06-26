using CommandLine;
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
            /* Templates
             *  Velentr.BASE
             *  Velentr.DUAL_SUPPORT
             *  Velentr.DUAL_SUPPORT_WITH_GENERIC
             *
             *
             * Example args:
             * args = new string[] { "generate", "-f", "-n", "CrossCommands", "-t", "Velentr.BASE", "-o", @"/Users/christianwebber/Dropbox/Projects/CrossCommands" }
             * 
               args = new string[] { "generate", "-n", "TEST_BASE", "-t", "Velentr.BASE", "-o", @"C:\Users\ricky\OneDrive\Computer\Documents\Templater\tmp" };
             *        Prepares the specified template solution
             *
               args = new string[] { "generate", "-n", "TEST_DUAL_SUPPORT", "-t", "Velentr.DUAL_SUPPORT", "-o", @"C:\Users\ricky\OneDrive\Computer\Documents\Templater\tmp" };
             *        Prepares the specified template solution
             *        
               args = new string[] { "generate", "-n", "TEST_DUAL_SUPPORT_WITH_GENERIC", "-t", "Velentr.DUAL_SUPPORT_WITH_GENERIC", "-o", @"C:\Users\ricky\OneDrive\Computer\Documents\Templater\tmp" };
             *        Prepares the specified template solution
             *
             *
             *
               args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.BASE", "-o", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES" };
             *        Prepares the specified template solution
             *
               args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.DUAL_SUPPORT", "-o", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES" };
             *        Prepares the specified template solution
             *
               args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.DUAL_SUPPORT_WITH_GENERIC", "-o", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES" };
             *        Prepares the specified template solution
             *
             *
               args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.BASE", "-o", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES" };
             *        Prepares the specified template solution
             *
               args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.BASE", "-o", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES", "s" };
             *        Prepares the specified template solution but skips cleanup
             *
               args = new string[] { "prepare", "-d", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES_BASE\Velentr.BASE", "-o", @"C:\Users\ricky\Dropbox\Projects\Templater-Templates\TEMPLATES", "i" };
             *        What-if preparation of the specified template solution
             *
             *
             *
               args = new string[] { "update-templates" };
             *        Checks for template updates
             *
               args = new string[] { "update-templates", "-f" };
             *        Forces template updates
             *
             *
             *
               args = new string[] { "report-issue" };
             *        Reports an issue
             *
             *
             *
               args = new string[] { "list-templates" };
             *        Lists all available templates
             *
               args = new string[] { "list-templates", "-q" };
             *        Lists all available templates but not details on version, etc.
             *
             *
             *
               args = new string[] { "--help" };
             *        Gets help!
            */
            //args = new string[] { "generate", "-n", "test", "-t", "Velentr.DUAL_SUPPORT", "-o", @"C:\Users\ricky\Dropbox\Projects\temp", "-c", @"C:\Users\ricky\OneDrive\Computer\Documents\Templater\SolutionConfigBackups\test_20230626100132.json" };

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