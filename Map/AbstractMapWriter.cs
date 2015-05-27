using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map
{
    public abstract class AbstractMapWriter
    {
        public abstract void WriteMap(string outputFile, BrushMap map);
    }
}
