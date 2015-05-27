using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace QMapConverter
{
    public class ModelBuilder
    {
        static readonly uint MASK_NONE = 0x0;
        static readonly uint MASK_POSITION = 0x1;
        static readonly uint MASK_NORMAL = 0x2;
        static readonly uint MASK_COLOR = 0x4;
        static readonly uint MASK_TEXCOORD1 = 0x8;
        static readonly uint MASK_TEXCOORD2 = 0x10;
        static readonly uint MASK_CUBETEXCOORD1 = 0x20;
        static readonly uint MASK_CUBETEXCOORD2 = 0x40;
        static readonly uint MASK_TANGENT = 0x80;
        static readonly uint MASK_BLENDWEIGHTS = 0x100;
        static readonly uint MASK_BLENDINDICES = 0x200;
        static readonly uint MASK_INSTANCEMATRIX1 = 0x400;
        static readonly uint MASK_INSTANCEMATRIX2 = 0x800;
        static readonly uint MASK_INSTANCEMATRIX3 = 0x1000;
        static readonly uint MASK_DEFAULT = 0xffffffff;
        static readonly uint NO_ELEMENT = 0xffffffff;

        public static void BuildModel(string outputFile, List<Map.Face> faces, List<string> materialSequence, bool forceCenter)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFile));
            BinaryWriter writer = new BinaryWriter(System.IO.File.Create(outputFile));

            writer.Write(System.Text.Encoding.ASCII.GetBytes("UMDL"));

            BoundingBox bounds = Map.Face.GetBounds(faces);

            // Split by texture/material
            IEnumerable<List<Map.Face>> faceLists = faces.GroupBy(p => p.TexName).Select(g => g.ToList());
            uint listCt = 0;
            int TotalFaceCt = 0;
            foreach (List<Map.Face> faceList in faceLists)
            {
                listCt++;
                TotalFaceCt += faceList.Count;
            }

            int currentFaceIdx = 0;
            Console.WriteLine(" ");
            Console.Write("Model Write Progress... 0/100%");

            // Write vertex buffers
            uint VertexMask = (MASK_POSITION | MASK_NORMAL | MASK_TEXCOORD1);
            writer.Write(listCt);
            foreach (List<Map.Face> faceList in faceLists)
            {
                // Write vertex buffer attributes
                materialSequence.Add(faceList[0].TexName);
                writer.Write((uint)(Map.Face.GetVertexCount(faceList))); // Face Count
                writer.Write((uint)VertexMask); // Vertex Mask
                writer.Write((uint)0); // Vertex Morph Start
                writer.Write((uint)0); // Vertex Moprh Ct

                // Write vertex data
                foreach (Map.Face face in faceList)
                {
                    ++currentFaceIdx;
                    float percentage = ((float)currentFaceIdx) / ((float)TotalFaceCt);
                    percentage *= 100;
                    Console.Write("\rModel Write Progress... {0}/100%", (int)(Math.Floor(percentage)));

                    foreach (Vector3 vert in face.Vertices)
                    {
                        if (forceCenter) {
                            writer.Write(vert.X - bounds.Center.X);
                            writer.Write(vert.Y - bounds.Center.Y);
                            writer.Write(vert.Z - bounds.Center.Z);
                        } else {
                            writer.Write(vert.X);
                            writer.Write(vert.Y);
                            writer.Write(vert.Z);
                        }

                        Vector3 normal = face.GetNormal();
                        if (Settings.FlipNormals)
                            normal = Vector3.Multiply(normal, -1);
                        writer.Write(normal.X);
                        writer.Write(normal.Y);
                        writer.Write(normal.Z);

                        Vector2 uv = face.GetTexCoord(vert);
                        writer.Write(uv.X);
                        writer.Write(uv.Y);
                    }
                }
            }

            // Write index buffers
            writer.Write(listCt);
            foreach (List<Map.Face> faceList in faceLists)
            {
                // Write index info
                ushort ct = (ushort)Map.Face.CountFaces(faceList);
                writer.Write((uint)(ct * 3)); // Index Count
                writer.Write((uint)2); // Index Size, unsigned short, 2 bytes

                int indexStart = 0;
                // Write index data, simple triangle list with unique vertices
                foreach (Map.Face face in faceList)
                {
                    for (int i = 0; i < face.Indices.Count; i += 3)
                    {
                        writer.Write((ushort)(indexStart + face.Indices[i]));
                        writer.Write((ushort)(indexStart + face.Indices[i + 1]));
                        writer.Write((ushort)(indexStart + face.Indices[i + 2]));
                    }
                    indexStart += face.Vertices.Count;
                }
                writer.Flush();
            }

            // Write geometries
            writer.Write(listCt);
            uint curRef = 0;
            foreach (List<Map.Face> faceList in faceLists)
            {
                writer.Write((uint)0); // Bone mapping count
                writer.Write((uint)1); // LOD Levels, only 1

                // Write geometry LOD info
                writer.Write(0.0f); // LOD Distance
                writer.Write((uint)0); // PrimtivieType, 0 == TRIANGLE_LIST
                writer.Write(curRef); // VB Ref
                writer.Write(curRef); // IB Ref
                writer.Write((uint)0); // IndexBuffer start
                writer.Write((uint)(Map.Face.CountFaces(faceList) * 3)); // Index Count to use

                ++curRef;
            }
            writer.Flush();

            // Vertex Morphs (empty)
            writer.Write((uint)0);

            // Bone count (empty)
            writer.Write((uint)0);

            // Write Bounds
            if (forceCenter) // Write bounds remapped to 0,0,0
            {
                writer.Write(0.0f - bounds.Dim.X * 0.5f);
                writer.Write(0.0f - bounds.Dim.Y * 0.5f);
                writer.Write(0.0f - bounds.Dim.Z * 0.5f);

                writer.Write(0.0f + bounds.Dim.X * 0.5f);
                writer.Write(0.0f + bounds.Dim.Y * 0.5f);
                writer.Write(0.0f + bounds.Dim.Z * 0.5f);
            }
            else // Write bounds as found
            {
                writer.Write(bounds.min.X);
                writer.Write(bounds.min.Y); // Swap Y and Z coords
                writer.Write(bounds.min.Z);

                writer.Write(bounds.max.X);
                writer.Write(bounds.max.Y); // Swap Y and Z coords
                writer.Write(bounds.max.Z);
            }

            // Write geometry centers
            
            for (uint i = 0; i < listCt; ++i)
            {
                // Vector3 (0,0,0) for center, center all geometries at origin
                if (forceCenter)
                {
                    writer.Write(0.0f);
                    writer.Write(0.0f);
                    writer.Write(0.0f);
                }
                else // Use bounds center
                {
                    Vector3 center = bounds.Center;
                    writer.Write(center.X);
                    writer.Write(center.Y);
                    writer.Write(center.Z);
                }
            }

            Console.WriteLine(" ");
            Console.WriteLine("Model Write Complete");

                writer.Flush();
            writer.Close();
        }

        static uint[] elementSize =
            {
                3 * sizeof(float), // Position
                3 * sizeof(float), // Normal
                4 * sizeof(char), // Color
                2 * sizeof(float), // Texcoord1
                2 * sizeof(float), // Texcoord2
                3 * sizeof(float), // Cubetexcoord1
                3 * sizeof(float), // Cubetexcoord2
                4 * sizeof(float), // Tangent
                4 * sizeof(float), // Blendweights
                4 * sizeof(char), // Blendindices
                4 * sizeof(float), // Instancematrix1
                4 * sizeof(float), // Instancematrix2
                4 * sizeof(float) // Instancematrix3
            };

        static uint GetVertexSize(uint elementMask)
        {
            uint vertexSize = 0;

            for (int i = 0; i < 13; ++i) //13, magic max vertex elements
            {
                if ((elementMask & (1 << i)) != 0)
                    vertexSize += elementSize[i];
            }

            return vertexSize;
        }
    }
}
