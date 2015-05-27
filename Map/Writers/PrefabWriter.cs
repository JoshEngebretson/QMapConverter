using QMapConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Writers
{
    [QMapConverter.WriterMode("prefab")]
    public class PrefabWriter : AbstractMapWriter
    {
        public override void WriteMap(string outputFile, BrushMap map)
        {
            SceneBuilder sb = new SceneBuilder(map, Settings.CellSize);
            sb.WriteScene(outputFile, Settings.ContentDir, "entity");
        }
    }
}
