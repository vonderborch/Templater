namespace Templater.Helpers
{
    internal static class ConsoleHelpers
    {
        /// <summary>
        /// Gets the input.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="defaultDisplayValue">The default display value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The input response.</returns>
        public static string GetInput(string message, string defaultDisplayValue = "", string defaultValue = "")
        {
            var defaultMessage = defaultDisplayValue == string.Empty ? string.Empty : $" ({defaultDisplayValue})";
            Console.Write($"{message}{defaultMessage}: ");
            var input = Console.ReadLine();
            var defaultReturn = defaultValue == string.Empty ? defaultDisplayValue : defaultValue;
            return string.IsNullOrEmpty(input) ? defaultReturn : input;
        }

        /// <summary>
        /// Gets whether the user agrees or not.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="defaultYes">if set to <c>true</c> [default yes].</param>
        /// <returns>True if Yes, False if No.</returns>
        public static bool GetYesNo(string message, bool defaultYes = true)
        {
            while (true)
            {
                Console.Write($"{message} ({(defaultYes ? "Y/n" : "y/N")}) ");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key == ConsoleKey.Y)
                {
                    return true;
                }
                else if (key.Key == ConsoleKey.N)
                {
                    return false;
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    return defaultYes;
                }
            }
        }
    }
}