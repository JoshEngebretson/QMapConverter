using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map
{
    public class Vertex
    {
        public Vector3 Position = new Vector3();
        public Vector3 Normal = new Vector3();
        public Vector2 UV = new Vector2();

        public Vertex() { }

        public Vertex(Vector3 pos)
        {
            Position = pos;
        }

        public Vertex(Vector3 pos, Vector2 uv)
        {
            Position = pos;
            UV = uv;
        }

        public Vertex(Vector3 pos, Vector3 norm, Vector2 uv)
        {
            Position = pos;
            Normal = norm;
            UV = uv;
        }

        public Vertex Clone()
        {
            Vertex ret = new Vertex();
            ret.Position = new Vector3(Position);
            ret.Normal = new Vector3(Normal);
            ret.UV = new Vector2(UV.X, UV.Y);
            return ret;
        }
    }
}
