using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toe;

namespace Map
{    
    public class BrushMap
    {
        public Dictionary<string, string> SceneProperties = new Dictionary<string, string>();
        public List<Entity> Entities = new List<Entity>();

        public BrushMap() { }
    }
}
