namespace Templater.Options
{
    internal abstract class AbstractOption
    {
        /// <summary>
        /// Executes what this option represents.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>The result of the execution.</returns>
        public abstract string Execute(AbstractOption option);
    }
}