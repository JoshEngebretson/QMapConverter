using QMapConverter;
using QMapConverter.SceneXML;
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
            Console.WriteLine("Urho3D Prefab Writer");
            Console.WriteLine("    QMapConverter prefab MapName.map OutputMapName.xml");
        }

        public override void WriteMap(BrushMap map)
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(" Urho3D Prefab Writer");
            Console.WriteLine("----------------------------------------------------------------");

            Console.WriteLine("Building material database");
            MaterialDatabase matDb = new MaterialDatabase(Settings.ContentDir);

            Scene scene = new Scene("node");
            Node entRoot = scene.CreateChild("Entities");

            // Write 'void' entity nodes
            using (QMapConverter.Util.ConsoleProgress prog = new QMapConverter.Util.ConsoleProgress("Writing entities", map.Entities.Count))
                for (int i = 0; i < map.Entities.Count; ++i)
                {
                    prog.Increment();
                    prog.Write();
                    Entity entity = map.Entities[i];
                    if (entity.Brushes.Count == 0)
                    {
                        Node brushNode = entRoot.CreateChild();
                        brushNode.WriteVariables(entity.Properties);
                    }
                }

            // Write world geometry elements
            Node geoNode = scene.CreateChild("Geometry");

            string outputPath = System.IO.Path.Combine(Settings.ContentDir, "Data");
            outputPath = System.IO.Path.Combine(outputPath, "Models");
            outputPath = System.IO.Path.Combine(outputPath, System.IO.Path.GetFileNameWithoutExtension(outputFile));
            outputPath = System.IO.Path.Combine(outputPath, "geo.mdl");

            SceneBuilder sb = new SceneBuilder(map, QMapConverter.Settings.CellSize);
            List<string> materials = new List<string>();
            sb.WriteModel(outputPath, materials);

            using (QMapConverter.Util.ConsoleProgress prog = new QMapConverter.Util.ConsoleProgress("Writing geometry", map.Entities.Count))
            {
                Component staticModel = geoNode.CreateComponent("StaticModel");
                string relPath = outputPath.Replace(Settings.ContentDir + "\\", "").Replace("\\","/");
                staticModel.SetAttribute("Model", String.Format("Model;{0}", relPath));
                StringBuilder matString = new StringBuilder();
                foreach (string m in materials)
                {
                    string matFile = matDb.GetMaterial(m);
                    if (matFile != null && matFile.Length > 0)
                    {
                        if (matString.Length > 0)
                            matString.AppendFormat(";Material;{0}", matFile);
                        else
                            matString.AppendFormat("Material;{0}", matFile);
                    }
                }
                if (matString.Length > 0)
                    staticModel.SetAttribute("Material", matString.ToString());
            }


            using (QMapConverter.Util.ConsoleProgress prog = new QMapConverter.Util.ConsoleProgress("Writing file", map.Entities.Count))
                scene.Save(outputFile);

            Console.WriteLine("File written: " + outputFile);
        }
    }
}
