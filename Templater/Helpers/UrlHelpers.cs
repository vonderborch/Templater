using System.Diagnostics;

namespace Templater.Helpers
{
    internal static class UrlHelpers
    {
        /// <summary>
        /// Opens the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="errorMessage">The error message.</param>
        public static void OpenUrl(string url, string errorMessage)
        {
            try
            {
                var ps = new ProcessStartInfo(url) { UseShellExecute = true, Verb = "open" };
                Process.Start(ps);
            }
            catch (Exception ex)
            {
                Console.WriteLine(errorMessage);
            }
        }
    }
}
