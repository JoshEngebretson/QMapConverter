using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map.Writers
{
    /// <summary>
    /// Writes a .map file using Quake 1's format
    /// </summary>
    [QMapConverter.WriterMode("q1")]
    public class QuakeWriter : AbstractMapWriter
    {
        public override void WriteMap(string outputFile, BrushMap map)
        {
            using (StreamWriter writer = new StreamWriter(System.IO.File.Create(outputFile)))
            {
                foreach (Entity e in map.Entities)
                    WriteEntity(writer, e);
            }
        }

        void WriteEntity(StreamWriter writer, Entity entity)
        {
            writer.WriteLine("{");
            foreach (string key in entity.Properties.Keys)
            {
                writer.WriteLine(String.Format("\"{0}\" \"1\"", key, entity.Properties[key]));
            }
            foreach (Brush brush in entity.Brushes)
            {
                WriteBrush(writer, brush);
            }
            writer.WriteLine("}");
        }

        void WriteBrush(StreamWriter writer, Brush brush)
        {
            writer.WriteLine("{");
            foreach (Face face in brush.Faces)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("( ");
                WriteVec3(sb, face.PlanePointA);
                sb.Append(" ) ( ");
                WriteVec3(sb, face.PlanePointB);
                sb.Append(" ) ( ");
                WriteVec3(sb, face.PlanePointC);
                sb.Append(" ) ");
                sb.Append(face.TexName);
                WriteVec2(sb, face.Offset);
                sb.Append(face.Angle);
                WriteVec2(sb, face.Scale);

                writer.WriteLine(sb.ToString());
            }
            writer.WriteLine("}");
        }

        void WriteVec3(StringBuilder sb, Vector3 v)
        {
            sb.AppendFormat("{0} {1} {2}", v.X, v.Y, v.Z);
        }

        void WriteVec2(StringBuilder sb, Vector2 v)
        {
            sb.AppendFormat("{0} {1}", v.X, v.Y);
        }
    }
}
