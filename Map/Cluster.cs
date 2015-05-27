using Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map
{
    public class Cluster
    {
        public List< List<Face> > ClusterFaces(BrushMap map)
        {
            List<List<Face>> ret = new List<List<Face>>();

            List<Face> AllFaces = new List<Face>();
            foreach (Entity e in map.Entities)
            {
                foreach (Brush b in e.Brushes)
                    AllFaces.AddRange(b.Faces);
            }

            IEnumerable<List<Face>> SortedFaces = AllFaces.GroupBy(p => p.TexName).Select(g => g.ToList());


            return ret;
        }
    }
}
