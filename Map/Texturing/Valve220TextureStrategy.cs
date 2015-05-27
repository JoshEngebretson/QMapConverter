using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map.Texturing
{
    /// <summary>
    /// Provides the U/V texture axes precomputed
    /// </summary>
    public class Valve220TextureStrategy : AbstractTextureStrategy
    {
        public override Toe.Vector2 GetTextureCoordinate(Face face, Toe.Vector3 point)
        {
            Vector2 ret = new Vector2();

            float XScale = face.Scale.X == 0.0f ? 1.0f : face.Scale.X;
            float YScale = face.Scale.Y == 0.0f ? 1.0f : face.Scale.Y;

            Vector3 scaledAxisX = Vector3.Divide(face.XTexVector.Value, XScale);
            Vector3 scaledAxisY = Vector3.Divide(face.YTexVector.Value, YScale);

            ret.X = (Vector3.Dot(point, scaledAxisX) + face.Offset.X) / 32.0f;
            ret.Y = (Vector3.Dot(point, scaledAxisY) + face.Offset.Y) / 32.0f;

            return ret;
        }
    }
}
