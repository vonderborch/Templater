﻿using CommandLine;
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
            //var cmd = @"generate -f -n Velentr.Core -t Velentr.DUAL_SUPPORT_WITH_GENERIC -c \\Mac\Home\Documents\Templater\SolutionConfigBackups\Velentr.Core.json  -o \\Mac\Dropbox\Projects\Velentr";
            //args = cmd.Split(" ");

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