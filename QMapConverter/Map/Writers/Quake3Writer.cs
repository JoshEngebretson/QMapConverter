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
    /// Writes a .map file using Quake 2/3s flags format and supports curves
    /// </summary>
    
    [QMapConverter.WriterMode("q3")]
    public class Quake3Writer : AbstractMapWriter
    {
        public override bool CheckArgs(string[] rawArgs, Dictionary<string, string> arguments)
        {
            try
            {
                outputFile = rawArgs[2];
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void PrintUsage()
        {
            Console.WriteLine("Quake 3 Writer");
            Console.WriteLine("    QMapConverter q3 MapName.map OutputMapName.map");
        }

        public override void WriteMap(BrushMap map)
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(" Quake 3: Arena Writer");
            Console.WriteLine("----------------------------------------------------------------");

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
            foreach (Patch patch in entity.Patches)
            {
                WritePatch(writer, patch);
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

        void WritePatch(StreamWriter writer, Patch patch)
        {
            writer.WriteLine("{");
            writer.WriteLine("patchDef2");
            writer.WriteLine("{");
            writer.WriteLine(patch.TextureName);
            writer.WriteLine(String.Format("( {0} {1} 0 0 0 )", patch.Rows, patch.Columns));
            writer.WriteLine("(");
            int patchIndex = 0;
            for (int i = 0; i < patch.Rows; ++i)
            {
                writer.Write("( ");
                for (int j = 0; j < patch.Columns; ++patchIndex)
                {
                    writer.Write(String.Format("( {0} {1} {2} {3} {4} ) ", 
                        patch.FlatVertices[patchIndex].Position.X,
                        patch.FlatVertices[patchIndex].Position.Y,
                        patch.FlatVertices[patchIndex].Position.Z,
                        patch.FlatVertices[patchIndex].UV.X,
                        patch.FlatVertices[patchIndex].UV.Y));
                }
                writer.Write(")\n");
            }
            writer.WriteLine(")");
            writer.WriteLine("}");
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
