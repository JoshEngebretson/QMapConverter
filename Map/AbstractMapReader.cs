using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map
{
    public abstract class AbstractMapReader
    {
        public abstract BrushMap ReadMap(string file, Dictionary<string,string> settings);
    }
}
