namespace Templater.Core
{
    public abstract class AbstractTemplater
    {
        public abstract string Prepare(PrepareOptions options);
        public abstract string Generate(GenerateOptions options);
    }
}