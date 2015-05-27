using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Writers
{
    [QMapConverter.WriterMode("info")]
    public class InfoWriter : AbstractMapWriter
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

        string GetFileSizeString(int size)
        {
            var units = new[] { "bytes", "kb", "mb", "gb", "tb" };
            var index = 0;
            while (size > 1024)
            {
                size /= 1024;
                index++;
            }
            return string.Format("{0} {1}", size, units[index]);
        }

        public override void PrintUsage()
        {
            Console.WriteLine("Info Writer");
            Console.WriteLine("    QMapConverter info MapName.map");
        }

        public override void WriteMap(BrushMap map)
        {
            int brushCount = 0;
            int patchCount = 0;
            int meshCount = 0;
            int faceCount = 0;
            int vertCount = 0;
            int triCount = 0;

            BSP bsp = new BSP(map);
            bsp.TestDivide();
            
            bsp.GridDivide(map, 6, 1, 6);

            //Sorting.SpaceSort ss = new Sorting.SpaceSort(map);

            Dictionary<string, int> textures = new Dictionary<string,int>();
            foreach (Entity e in map.Entities)
            {
                foreach (Brush b in e.Brushes)
                {
                    brushCount++;
                    faceCount += b.Faces.Count;
                    foreach (Face f in b.Faces)
                    {
                        vertCount += f.Vertices.Count;
                        if (textures.ContainsKey(f.TexName))
                            textures[f.TexName] = textures[f.TexName] + 1;
                        else
                            textures[f.TexName] = 1;
                        triCount += f.Indices.Count / 3;
                    }
                }
                foreach (Patch p in e.Patches)
                {
                    if (p.LiteralMesh)
                        meshCount++;
                    else
                        patchCount++;
                    vertCount += p.Columns * p.Rows;
                    triCount += p.Columns * p.Rows * 2;
                    if (textures.ContainsKey(p.TextureName))
                        textures[p.TextureName] = textures[p.TextureName] + 1;
                    else
                        textures[p.TextureName] = 1;
                }
            }

            int texOnce = 0;
            int texMany = 0;
            foreach (string s in textures.Keys)
            {
                if (textures[s] > 1)
                    texMany++;
                else
                    texOnce++;
            }

            Console.WriteLine("");
            Console.WriteLine("Map Info:");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("GEOMETRY");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(String.Format("  {0} entities", map.Entities.Count));
            Console.WriteLine(String.Format("  {0} brushes", brushCount));
            Console.WriteLine(String.Format("        {0} faces", faceCount));
            Console.WriteLine(String.Format("  {0} patches", patchCount));
            Console.WriteLine(String.Format("  {0} meshes", meshCount));
            Console.WriteLine(String.Format("  {0} vertices", vertCount));
            Console.WriteLine(String.Format("  {0} triangles", triCount));
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("TEXTURES");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(String.Format("  {0} Total Textures", textures.Keys.Count));
            Console.WriteLine(String.Format("  {0} Reused", texMany));
            Console.WriteLine(String.Format("  {0} Unique", texOnce));
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("MEMORY ESTIMATE");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(String.Format("  {0} Vertices", GetFileSizeString(vertCount * sizeof(float) * 5)));
            Console.WriteLine(String.Format("  {0} Indices", GetFileSizeString(triCount * 3 * sizeof(short))));
            Console.WriteLine(String.Format("  {0} All Geometry", GetFileSizeString(vertCount * sizeof(float) * 5 + triCount * 3 * sizeof(short))));
            Console.WriteLine(String.Format("  {0} Textures", map.Entities.Count));
        }
    }
}
