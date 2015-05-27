using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map
{
    public abstract class AbstractMapWriter
    {
        public abstract void WriteMap(BrushMap map);
        public abstract bool CheckArgs(string[] rawArgs, Dictionary<string, string> arguments);
        public abstract void PrintUsage();

        public String outputFile;
    }
}
