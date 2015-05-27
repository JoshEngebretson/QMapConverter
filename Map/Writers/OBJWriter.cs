using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;
using QMapConverter;
using QMapConverter.Util;
using System.IO;

namespace Map.Writers
{
    [QMapConverter.WriterMode("obj")]
    public class OBJWriter : AbstractMapWriter
    {
        public override void WriteMap(string outputFile, BrushMap map)
        {
            using (StreamWriter stream = new StreamWriter(System.IO.File.Create(outputFile), UTF8Encoding.UTF8))
            {
                List<Map.Face> AllFaces = new List<Map.Face>();
                foreach (Map.Entity e in map.Entities)
                {
                    foreach (Map.Brush b in e.Brushes)
                        AllFaces.AddRange(b.Faces);
                }

                Console.WriteLine("Sorting faces...");
                IEnumerable<List<Map.Face>> SortedFaces = AllFaces.GroupBy(p => p.TexName).Select(g => g.ToList());
                int GroupCt = 0;
                foreach (List<Map.Face> faces in SortedFaces)
                    ++GroupCt;

                // Write "objects" to OBJ
                int CurrentIdx = 0;
                List<Brush> brushes = new List<Brush>();
                foreach (Entity e in map.Entities)
                    brushes.AddRange(e.Brushes);
                using (ConsoleProgress modelProgress = new ConsoleProgress(String.Format("Writing Meshes", CurrentIdx), SortedFaces.Count()))
                {
                    int indexStart = 1;
                    
                    foreach (Brush b in brushes)
                    //foreach (List<Map.Face> faces in SortedFaces)
                    {
                        List<Face> faces = b.Faces;
                        ++CurrentIdx;
                        modelProgress.Increment();
                        modelProgress.Write();

                        stream.Write(String.Format("o Mesh{0}\n", CurrentIdx));

                        int FaceCount = Map.Face.CountFaces(faces);

                        // Vertex Positions
                        foreach (Map.Face face in faces)
                        {
                            foreach (Vector3 vert in face.Vertices)
                            {
                                WriteVec3(stream, "v", vert);
                            }
                        }

                        // Texture Coordinates
                        foreach (Map.Face face in faces)
                        {
                            foreach (Vector3 vert in face.Vertices)
                            {
                                WriteVec2(stream, "vt", face.GetTexCoord(vert));
                            }
                        }

                        // Vertex Normals
                        foreach (Map.Face face in faces)
                        {
                            Vector3 norm = face.GetNormal().Multiply(-1);
                            foreach (Vector3 vert in face.Vertices)
                            {
                                WriteVec3(stream, "vn", norm);
                            }
                        }

                        foreach (Map.Face face in faces)
                        {
                            for (int i = 0; i < face.Indices.Count; i += 3)
                            {
                                stream.Write("f {0} {1} {2}\n", 
                                    indexStart + face.Indices[i], 
                                    indexStart + face.Indices[i + 1], 
                                    indexStart + face.Indices[i + 2]);
                            }
                            indexStart += face.Vertices.Count;
                        }
                    }

                    List<Patch> patches = new List<Patch>();
                    foreach (Entity e in map.Entities)
                    {
                        foreach (Patch patch in e.Patches)
                            patches.Add(patch);
                    }

                    int currentPatch = 0;
                    //foreach (Patch p in patches)
                    //{
                    //    ++currentPatch;
                    //    stream.Write(String.Format("o Patch{0}\n", currentPatch));
                    //    if (p.LiteralMesh)
                    //    {
                    //        for (int x = 0; x < p.Columns; ++x)
                    //        {
                    //            for (int y = 0; y < p.Rows; ++y)
                    //            {
                    //                PatchVert vert = p.FlatVertices[x + y * p.Columns];
                    //                stream.Write(String.Format("v {0} {1} {2}\n", vert.Position.X, vert.Position.Y, vert.Position.Z));
                    //            }
                    //        }
                    //        for (int x = 0; x < p.Columns; ++x)
                    //        {
                    //            for (int y = 0; y < p.Rows; ++y)
                    //            {
                    //                PatchVert vert = p.FlatVertices[x + y * p.Columns];
                    //                stream.Write(String.Format("vt {0} {1}\n", vert.UV.X, vert.UV.Y));
                    //            }
                    //        }
                    //        p.BuildIndices();
                    //        for (int i = 0; i < p.index.Count; i += 3)
                    //        {
                    //            stream.Write(String.Format("f {0} {1} {2}\n", 
                    //                ((ushort)indexStart + p.index[i]),
                    //                ((ushort)indexStart + p.index[i + 1]),
                    //                ((ushort)indexStart + p.index[i + 2])));
                    //        }
                    //        indexStart += p.index.Count;
                    //    } else {
                    //        p.Generate(1);
                    //        for (int i = 0; i < p.vertex.Count; ++i)
                    //        {
                    //            stream.Write(String.Format("v {0} {1} {2}\n", p.vertex[i].X, p.vertex[i].Y, p.vertex[i].Z));
                    //        }
                    //        for (int i = 0; i < p.uvs.Count; ++i)
                    //        {
                    //            stream.Write(String.Format("vt {0} {1}\n", p.uvs[i].X, p.uvs[i].Y));
                    //        }
                    //        for (int i = 0; i < p.index.Count; i += 3)
                    //        {
                    //            stream.Write(String.Format("f {0} {1} {2}\n", 
                    //                ((ushort)indexStart + p.index[i]),
                    //                ((ushort)indexStart + p.index[i + 1]),
                    //                ((ushort)indexStart + p.index[i + 2])));
                    //        }
                    //    }
                    //}
                }

                stream.Flush();
            }
        }

        static void WriteVec3(StreamWriter stream, string term, Vector3 vert)
        {
            stream.Write("{0} {1:0.000000} {2:0.000000} {3:0.000000}\n", term, vert.X, vert.Y, vert.Z);
        }

        static void WriteVec2(StreamWriter stream, string term, Vector2 vert)
        {
            stream.Write("{0} {1:0.000000} {2:0.000000}\n", term, vert.X, vert.Y);
        }
    }
}
