using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map
{
    public enum EntityType
    {
        Brush,
        Patch,
        Mesh
    }

    public class Entity
    {
        public EntityType Kind = EntityType.Brush;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public List<Brush> Brushes = new List<Brush>();
        public List<Patch> Patches = new List<Patch>();
    }
}
