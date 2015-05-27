using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Toe;
using QMapConverter.SceneXML;

namespace QMapConverter
{
    public class SceneBuilder
    {
        Map.BrushMap map;
        Vector3 MaxCoords = new Vector3();
        Vector3 MinCoords = new Vector3();
        Vector3 DivisionUnit = new Vector3();

        public SceneBuilder(Map.BrushMap aMap, Vector3 locality)
        {
            map = aMap;
            DivisionUnit = locality;
        }

        public void WriteModel(string outputFile, List<string> materialSequence)
        {
            // Write an entire model
            List<Map.Face> faces = new List<Map.Face>();
            foreach (Map.Entity entity in map.Entities)
            {
                foreach (Map.Brush brush in entity.Brushes)
                {
                    foreach (Map.Face face in brush.Faces)
                    {
                        faces.Add(face);
                    }
                }
            }
            ModelBuilder.BuildModel(outputFile, faces, materialSequence, true);
        }

        public void WriteScene(string outputFile, string contentDir, string rootName)
        {
            Scene scene = new Scene(rootName);

            Node entRoot = scene.CreateChild("Entities");
        
        // Write 'void' entity nodes
            foreach (Map.Entity entity in map.Entities)
            {
                if (entity.Brushes.Count == 0)
                {
                    Node brushNode = entRoot.CreateChild();
                    brushNode.WriteVariables(entity.Properties);
                }
            }

        // Write world geometry elements
            Node geoNode = scene.CreateChild("Geometry");
            foreach (Map.Entity entity in map.Entities)
            {
                Node entNode = geoNode.CreateChild();
                entNode.WriteVariables(entity.Properties);
                // Geometry entity
                if (entity.Brushes.Count != 0)
                {
                    Node brush = geoNode.CreateChild();
                }
            }

            scene.Save(outputFile);
        }
    }
}
