using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Templater.Options
{
    internal abstract class AbstractOption
    {
        public abstract string Execute(AbstractOption option);
    }
}
