using QMapConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Writers
{
    [QMapConverter.WriterMode("model")]
    public class UMDLWriter : AbstractMapWriter
    {
        public override void WriteMap(string outputFile, BrushMap map)
        {
            SceneBuilder sb = new SceneBuilder(map, QMapConverter.Settings.CellSize);
            sb.WriteModel(outputFile, QMapConverter.Settings.MaterialSourceDir);
        }
    }
}
