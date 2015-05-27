using QMapConverter;
using QMapConverter.SceneXML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Writers
{
    //DISABLE SCENE WRITING FOR NOW [QMapConverter.WriterMode("scene")]
    public class SceneWriter  : AbstractMapWriter
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
            Console.WriteLine("Urho3D Scene Writer");
            Console.WriteLine("    QMapConverter scene MapName.map MapName.xml");
        }

        public override void WriteMap(BrushMap map)
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(" Urho3D Scene Writer");
            Console.WriteLine("----------------------------------------------------------------");

            Console.WriteLine("Building material database");
            MaterialDatabase matDb = new MaterialDatabase(Settings.ContentDir);

            Scene scene = new Scene("scene");
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

            using (QMapConverter.Util.ConsoleProgress prog = new QMapConverter.Util.ConsoleProgress("Writing geometry", map.Entities.Count))
            foreach (Map.Entity entity in map.Entities)
            {
                prog.Increment();
                prog.Write();

                Node entNode = geoNode.CreateChild();
                entNode.WriteVariables(entity.Properties);
                // Geometry entity
                if (entity.Brushes.Count != 0)
                {
                    Node brush = geoNode.CreateChild();
                }
            }


            using (QMapConverter.Util.ConsoleProgress prog = new QMapConverter.Util.ConsoleProgress("Writing file", map.Entities.Count))
                scene.Save(outputFile);

            Console.WriteLine("File written: " + outputFile);
        }
    }
}
