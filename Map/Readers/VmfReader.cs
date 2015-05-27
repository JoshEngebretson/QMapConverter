using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Readers
{
    /// <summary>
    /// Reads Vmf format maps, which are a slight structural change to Valve 220 .map format
    /// </summary>
    public class VmfReader : AbstractMapReader
    {
        public override BrushMap ReadMap(string file, Dictionary<string, string> settings)
        {
            throw new NotImplementedException();
        }
    }
}
