using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map.Texturing
{
    /// <summary>
    /// Similar, however provides world units of each faces texture space instead of scale
    /// </summary>
    public class Quake3TextureStrategy : QuakeTextureStrategy
    {
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

            // Treyarch format includes the image basis, in Q1/Q2/Q3/HL this is based on texture width
            ret.X = (Vector3.Dot(point, scaledAxisX) + face.Offset.X) / face.ImageBasis.Value.X;
            ret.Y = (Vector3.Dot(point, scaledAxisY) + face.Offset.Y) / face.ImageBasis.Value.Y;

            return ret;
        }
    }
}
