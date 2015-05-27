using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Sorting
{
    public static class TexSort
    {
        public static IEnumerable<List<Face>> FacesByTexture(Brush brush)
        {
            return brush.Faces.GroupBy(p => p.TexName).Select(g => g.ToList());
        }

        public static IEnumerable<List<Face>> FacesByTexture(BrushMap map)
        {
            List<Map.Face> AllFaces = new List<Map.Face>();
            foreach (Map.Entity e in map.Entities)
            {
                foreach (Map.Brush b in e.Brushes)
                    AllFaces.AddRange(b.Faces);
            }
            Console.WriteLine("Sorting faces...");
            return AllFaces.GroupBy(p => p.TexName).Select(g => g.ToList());
        }

        public static IEnumerable<List<Face>> FacesByTexture(List<Brush> brushes)
        {
            List<Face> AllFaces = new List<Face>();
            foreach (Map.Brush b in brushes)
                AllFaces.AddRange(b.Faces);
            Console.WriteLine("Sorting faces...");
            return AllFaces.GroupBy(p => p.TexName).Select(g => g.ToList());
        }

        public static IEnumerable<List<Face>> FacesByTexture(List<Entity> entities)
        {
            List<Face> AllFaces = new List<Face>();
            foreach (Map.Entity e in entities)
            {
                foreach (Map.Brush b in e.Brushes)
                    AllFaces.AddRange(b.Faces);
            }
            Console.WriteLine("Sorting faces...");
            return AllFaces.GroupBy(p => p.TexName).Select(g => g.ToList());
        }
    }
}
