using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map.Texturing
{
    /// <summary>
    /// Basis of all Quake derived texture strategies
    /// </summary>
    public class QuakeTextureStrategy : AbstractTextureStrategy
    {
        protected static readonly Vector3[] BaseAxes = { 
            Vector3.UnitZ,          Vector3.UnitX,  new Vector3(0,-1,0),
            new Vector3(0,0,-1),    Vector3.UnitX,  new Vector3(0,-1,0),
            Vector3.UnitX,          Vector3.UnitY,  new Vector3(0,0,-1),
            new Vector3(-1,0,0),    Vector3.UnitY,  new Vector3(0,0,-1),
            Vector3.UnitY,          Vector3.UnitX,  new Vector3(0,0,-1),
            new Vector3(0,-1,0),    Vector3.UnitX,  new Vector3(0,0,-1)
        };

        public int GetAxisBaseIndex(Face face)
        {
            Vector3 normal = face.GetNormal();
            int bestIndex = 0;
            float bestDot = 0.0f;
            for (int i = 0; i < 6; i++)
            {
                float dot = 0.0f;
                Vector3.Dot(ref normal, ref BaseAxes[i * 3], out dot);
                if (dot > bestDot)
                { // no need to use -altaxis for qbsp
                    bestDot = dot;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        public override Vector2 GetTextureCoordinate(Face face, Vector3 point)
        {
            Vector2 ret = new Vector2();

            // Calculate coordinate axes if we don't have them (Quake, HalfLife/Valve200 will include them)
            if (!face.XTexVector.HasValue || !face.YTexVector.HasValue)
            {
                Vector3 normal = face.GetNormal();

                int bestIndex = GetAxisBaseIndex(face);

                Vector3 xAxis = BaseAxes[bestIndex * 3 + 1];
                Vector3 yAxis = BaseAxes[bestIndex * 3 + 2];
                int planeNormIndex = (bestIndex / 2) * 6;
                int faceNormIndex = bestIndex * 3;

                float radAngle = face.Angle / 180.0f * 3.1415f;

                Quaternion quat = new Quaternion(BaseAxes[planeNormIndex], planeNormIndex == 12 ? -radAngle : radAngle);
                Quaternion con = quat;
                con.Conjugate();

                Quaternion xQuat = quat;
                xQuat.Normalized();
                xQuat = Quaternion.Multiply(Quaternion.Multiply(xQuat, new Quaternion(xAxis, 0)), con);
                Quaternion yQuat = quat;
                yQuat.Normalized();
                yQuat = Quaternion.Multiply(Quaternion.Multiply(yQuat, new Quaternion(yAxis, 0)), con);

                face.XTexVector = xQuat.Xyz;
                face.YTexVector = yQuat.Xyz;
            }

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
