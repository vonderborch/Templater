using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Templater.Helpers
{
    internal static class ConsoleHelpers
    {
        public static string GetInput(string message, string defaultDisplayValue = "", string defaultValue = "")
        {
            var defaultMessage = defaultDisplayValue == string.Empty ? string.Empty : $" ({defaultDisplayValue})";
            Console.Write($"{message}{defaultMessage}: ");
            var input = Console.ReadLine();
            var defaultReturn = defaultValue == string.Empty ? defaultDisplayValue : defaultValue;
            return string.IsNullOrEmpty(input) ? defaultReturn : input;
        }

        public static bool GetYesNo(string message, bool defaultYes)
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
