using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toe;

namespace Map.Readers
{
    public class QuakeReader2 : AbstractMapReader
    {
        static readonly char[] PATCH_SPLIT = { ' ', 'v', 't', 'c', '(', ')' };
        static readonly char[] SPLIT_CHARS = { '(', ' ', ')' };
        static readonly char[] PROPERTY_SPLIT_CHARS = { '\"' };

        public override BrushMap ReadMap(string inputFile, Dictionary<string, string> settings)
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("Quake Reader 2");
            Console.WriteLine("----------------------------------------------------------------");

            Vector3 scale = QMapConverter.Settings.MapScale;

            Console.WriteLine(String.Format("Reading file: {0}", inputFile));
            BrushMap map = new BrushMap();
            List<String> fileData = new List<String>(System.IO.File.ReadLines(inputFile));
            int braceDepth = 0;

            Entity currentEntity = null;
            Brush currentBrush = null;
            Patch currentPatch = null;

            Console.Write("Map Load Progress...0/100%", 0 / fileData.Count);
            for (int lineNum = 0; lineNum < fileData.Count; ++lineNum)
            {
                string line = fileData[lineNum];
                float percentage = ((float)lineNum+1) / ((float)fileData.Count);
                percentage *= 100;
                Console.Write(String.Format("\rMap Load Progress...{0}/100%", Math.Floor(percentage)));
                string shortLine = line.Trim();

                // Skip over comment lines
                if (shortLine.StartsWith("//"))
                    continue;

                if (shortLine.CompareTo("{") == 0)
                {
                    ++lineNum;
                    ParseEntity(map, ref lineNum, fileData);
                }
            }
            Console.WriteLine(" ");
            Console.WriteLine("Map Load Complete");
            return map;
        }

        void ParseEntity(BrushMap map, ref int line, List<string> lines)
        {
            Entity entity = new Entity();
            int braceStack = 0;
            for (; line < lines.Count; ++line)
            {
                string text = lines[line];
                string shortLine = text.Trim();
                if (text.StartsWith("//"))
                {
                    continue;
                } 
                else if (lines[line].Contains("}"))
                {
                    --braceStack;
                    if (braceStack < 0)
                        break;
                } else if (lines[line].Contains("{")) {
                    ++braceStack;
                }
                else if (shortLine.Contains("curve") || shortLine.Contains("patch") || shortLine.Contains("mesh"))
                {
                    ParsePatch(entity, ref line, lines);
                }
                else if (braceStack > 0) {
                    ParseBrush(entity, ref line, lines);
                }
                else
                {
                    string[] terms = text.Split(PROPERTY_SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);
                    if (terms.Length == 2)
                        entity.Properties[terms[0].Trim()] = terms[1].Trim();
                    else
                        entity.Properties[terms[0].Trim()] = "true";
                }
            }
            map.Entities.Add(entity);
        }

        void ParseBrush(Entity entity, ref int line, List<string> lines)
        {
            Brush brush = new Brush();
            for (; line < lines.Count; ++line)
            {
                string text = lines[line];
                string shortLine = text.Trim();
                if (shortLine.Contains("}"))
                    break;
                if (shortLine.Contains("("))
                {
                    Face face = new Face(shortLine, QMapConverter.Settings.MapScale);
                    brush.Faces.Add(face);
                }
            }
            entity.Brushes.Add(brush);
        }

        void ParsePatch(Entity entity, ref int line, List<string> lines)
        {
            //++line;
            string defLine = lines[line].ToLower();
            Patch patch = new Patch(defLine.Contains("mesh"));
            bool tFormat = defLine.Contains("mesh") || defLine.Contains("curve");

            // Skip the definition line, skip the {
            line += 2;

            if (!tFormat)
            {
                // Get the texture name
                patch.TextureName = lines[line].Trim();
                ++line;
                // Get the patch dimensions
                string[] sizeTerms = lines[line].Split(SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);
                patch.Rows = int.Parse(sizeTerms[0]);
                patch.Columns = int.Parse(sizeTerms[1]);
                ++line;
            }
            else
            {
                for (; line < lines.Count; ++line)
                {
                    if (lines[line].Contains("lightmap") || lines[line].Contains("content"))
                        continue;
                    else
                    {
                        if (lines[line].Contains("("))
                        {
                            break;
                        }
                        string[] terms = lines[line].Trim().Split(' ');
                        if (terms.Length == 1)
                            patch.TextureName = terms[0];
                        else if (terms.Length == 4)
                        {
                            patch.Rows = int.Parse(terms[0]);
                            patch.Columns = int.Parse(terms[1]);
                        }
                    }
                }
            }

            int jumpCt = 0;
            for (; line < lines.Count; ++line)
            {
                string text = lines[line];
                string shortLine = lines[line].Trim();

                if (shortLine.Contains("}"))
                {
                    ++jumpCt;
                    if (jumpCt == 2) // Leave after second } encounters { patchDef { "content" } } <--
                        break;
                    continue;
                }
                else
                {
                    string[] terms = shortLine.Split(PATCH_SPLIT, StringSplitOptions.RemoveEmptyEntries);
                    if (tFormat)
                    {
                        if (shortLine.Contains('v'))
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
                            patch.FlatVertices.Add(vert);
                        }
                    }
                    else if (terms.Length > 0)
                    {
                        // Id format
                        if (shortLine.Count(c => c == '(') > 0)
                        {
                            List<PatchVert> rowVerts = new List<PatchVert>();
                            // ( (x y z u v) (x y z u v) (x y z u v) )
                            for (int i = 0; i < patch.Columns; ++i)
                            {
                                PatchVert vert = new PatchVert();
                                int startSub = i * 5;
                                // (x y z u v)
                                vert.Position = Face.ParseVec3(terms, startSub);
                                vert.UV = Face.ParseVec2(terms, startSub + 3);
                                rowVerts.Add(vert);
                                patch.FlatVertices.Add(vert);
                            }
                            patch.PatchVertices.Add(rowVerts);
                        }
                    }
                }
            }
            entity.Patches.Add(patch);
        }
    }
}
