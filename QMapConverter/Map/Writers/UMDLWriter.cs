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
        public override bool CheckArgs(string[] rawArgs, Dictionary<string, string> arguments)
        {
            try
            {
                outputFile = rawArgs[2];
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void PrintUsage()
        {
            Console.WriteLine("Urho3D Model Writer");
            Console.WriteLine("    QMapConverter model MapName.map MapName.mdl <args>");
        }

        public override void WriteMap(BrushMap map)
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(" Urho3D Model Writer");
            Console.WriteLine("----------------------------------------------------------------");

            SceneBuilder sb = new SceneBuilder(map, QMapConverter.Settings.CellSize);
            List<string> matlist = new List<string>();
            sb.WriteModel(outputFile, matlist);
        }
    }
}
