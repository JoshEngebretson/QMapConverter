using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map
{
    public abstract class AbstractTextureStrategy
    {
        public abstract Vector2 GetTextureCoordinate(Face face, Vector3 point);
    }
}
