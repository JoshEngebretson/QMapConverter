using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map.Texturing
{
    /// <summary>
    /// GtkRadiant Brush-Primitives provide the first 2 rows a texture matrix
    /// </summary>
    public class GtkTextureStrategy : QuakeTextureStrategy
    {
        public override Vector2 GetTextureCoordinate(Face face, Vector3 point)
        {
            Matrix3 texMat = face.TextureMatrix.Value;
            if (!face.XTexVector.HasValue || !face.YTexVector.HasValue)
            {
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

            float x = Vector3.Dot(face.XTexVector.Value, point);
            float y = Vector3.Dot(face.YTexVector.Value, point);
            float u = texMat.M11 * x + texMat.M12 * y + texMat.M13;
            float v = texMat.M21 * x + texMat.M22 * y + texMat.M23;

            return new Vector2(u, v);
        }
    }
}
