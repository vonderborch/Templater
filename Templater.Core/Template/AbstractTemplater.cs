using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Templater.Core.Template;

namespace Templater.Core.Template
{
    internal abstract class AbstractTemplater
    {
        public abstract string Prepare(PrepareOptions options);

        public abstract string Generate(GenerateOptions options);
    }
}
