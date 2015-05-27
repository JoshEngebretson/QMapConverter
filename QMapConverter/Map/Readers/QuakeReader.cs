using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toe;

namespace Map.Readers
{
    /// <summary>
    /// Reads Q1, Q2, Q3A, Treyarch, and Valve 220 .map files
    /// </summary>
    public class QuakeReader : AbstractMapReader
    {
        static readonly char[] SPLIT_CHARS = {'(', ' ', ')'};

        public override BrushMap ReadMap(string inputFile, Dictionary<string, string> settings)
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(" Quake Reader");
            Console.WriteLine("----------------------------------------------------------------");

            Vector3 scale = new Vector3(1,1,1);

            if (settings.ContainsKey("scale"))
                scale = QMapConverter.Util.MathExt.Vector3FromString(settings["scale"]);

            BrushMap map = new BrushMap();
            List<String> fileData = new List<String>(System.IO.File.ReadLines(inputFile));
            int braceDepth = 0;
            int lineNum = 0;

            Entity currentEntity = null;
            Brush currentBrush = null;
            Patch currentPatch = null;


            using (QMapConverter.Util.ConsoleProgress prog = new QMapConverter.Util.ConsoleProgress("Loading Map", fileData.Count))
            foreach (string line in fileData)
            {
                ++lineNum;
                prog.Increment();
                prog.Write();
                string shortLine = line.Trim();

                // Skip over comment lines
                if (shortLine.StartsWith("//"))
                    continue;

                if (shortLine.CompareTo("{") == 0)
                {
                    ++braceDepth;
                    if (braceDepth == 1)
                        currentEntity = new Entity();
                    else if (braceDepth == 2)
                        currentBrush = new Brush();
                }
                else if (shortLine.CompareTo("}") == 0)
                {
                    --braceDepth;
                    if (braceDepth == 0 && currentEntity != null)
                    {
                        map.Entities.Add(currentEntity);
                        currentEntity = null;
                    }
                    else if (braceDepth == 1 && (currentBrush != null || currentPatch != null) && currentEntity != null)
                    {
                        if (currentBrush != null)
                        {
                            currentEntity.Brushes.Add(currentBrush);
                            currentBrush = null;
                        } 
                        else if (currentPatch != null)
                        {
                            currentEntity.Patches.Add(currentPatch);
                            currentPatch = null;
                        }
                    }
                }
                else
                {
                    if (currentBrush != null)
                    {
                        if (shortLine.ToLower().Equals("curve") || shortLine.ToLower().Contains("patchdef") || shortLine.ToLower().Equals("mesh"))
                        {
                            // Cancel the brush, it's either a patch or a mesh
                            currentBrush = null;
                            currentPatch = new Patch(shortLine.ToLower().Equals("mesh"));
                            continue;
                        }
                        //try
                        {
                            if (!shortLine.Contains('('))
                                continue;
                            Face face = new Face(shortLine, scale);
                            currentBrush.Faces.Add(face);
                        }
                        //catch (Exception ex)
                        //{
                        //    Console.WriteLine(String.Format("ERROR: line {0}", lineNum));
                        //    Console.WriteLine(ex.Message);
                        //    Environment.FailFast("Map Parse Error");
                        //    return null;
                        //}
                    }
                    else if (currentPatch != null)
                    {
                        int termCt = shortLine.Split(SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries).Count();
                        if (currentPatch.Columns == -1 && termCt == 5 || termCt == 4)
                        {
                            string[] terms = shortLine.Split(SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);
                            currentPatch.Rows = int.Parse(terms[0]);
                            currentPatch.Columns = int.Parse(terms[1]);
                        }
                        else if (termCt == 1 && currentPatch.TextureName.Length == 0)
                        {
                            currentPatch.TextureName = shortLine;
                        }
                        else
                        {
                            // Characters on which to split
                            char[] PATCH_SPLIT = { ' ', 'v', 't', 'c', '(', ')' };
                            string[] terms = shortLine.Split(PATCH_SPLIT, StringSplitOptions.RemoveEmptyEntries);
                            if (terms.Length > 0)
                            {
                                // Id format
                                if (shortLine.Count(c => c == '(') > 0)
                                {
                                    List<PatchVert> rowVerts = new List<PatchVert>();
                                    // ( (x y z u v) (x y z u v) (x y z u v) )
                                    for (int i = 0; i < currentPatch.Columns; ++i)
                                    {
                                        PatchVert vert = new PatchVert();
                                        int startSub = i * 5;
                                        // (x y z u v)
                                        vert.Position = Face.ParseVec3(terms, startSub);
                                        vert.UV = Face.ParseVec2(terms, startSub + 3);
                                        rowVerts.Add(vert);
                                        currentPatch.FlatVertices.Add(vert);
                                    }
                                    currentPatch.PatchVertices.Add(rowVerts);
                                }
                                // Treyarch format
                                else if (shortLine.Contains('v'))
                                {
                                    PatchVert vert = new PatchVert();
                                    vert.Position = Face.ParseVec3(terms, 0);
                                    // If we contain a color
                                    if (shortLine.Contains('c'))
                                    {
                                        vert.Color = Face.ParseVec3(terms, 3);
                                        vert.Alpha = float.Parse(terms[6]);
                                        vert.UV = Face.ParseVec2(terms, 7);
                                    }
                                    else
                                        vert.UV = Face.ParseVec2(terms, 3);
                                    currentPatch.FlatVertices.Add(vert);
                                }
                            }
                        }
                    }
                    else if (currentEntity != null)
                    {
                        // Brush Primitives - GtkRadiant format
                        if (shortLine.ToLower().Equals("brushdef"))
                        {
                            currentEntity.Kind = EntityType.Brush;
                        }
                        // Patch
                        else if (shortLine.ToLower().Equals("patchdef2") || shortLine.ToLower().Equals("curve"))
                        {
                            currentEntity.Kind = EntityType.Patch;
                        }
                        else if (shortLine.ToLower().Equals("mesh"))
                        {
                            currentEntity.Kind = EntityType.Mesh;
                        }
                        else
                        {
                            Regex regex = new Regex(@"\w+|""[\w\s]*""");
                            List<string> terms = new List<string>(shortLine.Split('"').ToList());
                            for (int i = 0; i < terms.Count; ++i)
                            {
                                if (String.IsNullOrWhiteSpace(terms[i]))
                                {
                                    terms.RemoveAt(i);
                                    --i;
                                }
                            }

                            // Insert KvP properties, if a Value is non-existent, insert 'true' as it's likely a flag
                            if (terms.Count == 2)
                                currentEntity.Properties[terms[0].Replace("\"", "")] = terms[1].Replace("\"", "");
                            else
                                currentEntity.Properties[terms[0].Replace("\"", "")] = "true";
                        }
                    }
                }
            }
            return map;
        }
    }
}
