using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map.Readers
{
    /// <summary>
    /// Reads Vmf format maps, which are a slight structural change to Valve 220 .map format
    /// </summary>
    public class VmfReader : AbstractMapReader
    {
        static readonly char[] SIDE_SPLIT_CHARS = { '(', ' ', ')', '[', ']' };
        static readonly char[] SPLIT_PROPERTIES = { '"' };

        public override BrushMap ReadMap(string inputFile, Dictionary<string, string> settings)
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(" Valve VMF Reader");
            Console.WriteLine("----------------------------------------------------------------");

            Vector3 scale = QMapConverter.Settings.MapScale;

            BrushMap map = new BrushMap();
            List<String> fileData = new List<String>(System.IO.File.ReadLines(inputFile));

            //using (QMapConverter.Util.ConsoleProgress prog = new QMapConverter.Util.ConsoleProgress("Loading map", fileData.Count))
            {
                for (int i = 0; i < fileData.Count; ++i)
                {
                    //prog.Increment();
                    //prog.Write();

                    string line = fileData[i];
                    string shortLine = fileData[i].Trim();
                    string shortLower = shortLine.ToLower();

                    if (shortLower.Equals("world"))
                        ParseWorld(map, ref i, fileData);
                    else if (shortLower.Equals("entity"))
                        ParseEntity(map, ref i, fileData);
                }
            }

            return map;
        }

        void ParseWorld(BrushMap map, ref int line, List<string> lines)
        {
            int braceDepth = 0;
            Entity worldEntity = new Entity();
            map.Entities.Add(worldEntity);

            for (; line < lines.Count; ++line)
            {
                string text = lines[line];
                string shortLine = text.Trim();
                string lowerLine = shortLine.ToLower();

                if (lowerLine.Contains("{"))
                    ++braceDepth;
                else if (lowerLine.Contains("}"))
                {
                    --braceDepth;
                    if (braceDepth == 0)
                        return;
                }
                else if (lowerLine.Equals("solid")) // Accept solid objects
                    ParseSolid(worldEntity, ref line, lines);
                else if (lowerLine.Equals("hidden")) // Ignore hidden objects?
                    SkipBraces(ref line, lines);
                else if (lowerLine.Equals("group")) // Ignore groups, they're about editor function
                    SkipBraces(ref line, lines);
                else if (braceDepth == 1) // Scene level properties
                    ParseProperty(shortLine, map.SceneProperties);
            }
        }

        void ParseSolid(Entity owner, ref int line, List<string> lines)
        {
            int braceDepth = 0;

            Brush brush = new Brush();
            owner.Brushes.Add(brush);
            for (; line < lines.Count; ++line)
            {
                string text = lines[line];
                string shortLine = text.Trim();
                string lowerLine = shortLine.ToLower();

                if (lowerLine.Contains("{"))
                {
                    ++braceDepth;
                }
                else if (lowerLine.Contains("}"))
                {
                    --braceDepth;
                    if (braceDepth == 0)
                        return;
                }
                if (lowerLine.Equals("side"))
                {
                    ParseSide(brush, ref line, lines);
                }
            }
        }

        void ParseSide(Brush brush, ref int line, List<string> lines)
        {
            int braceCt = 0;
            Dictionary<string, string> stubs = new Dictionary<string, string>();
            for (; line < lines.Count; ++line)
            {
                string text = lines[line];
                string shortLine = text.Trim();
                string lowerLine = shortLine.ToLower();

                if (lowerLine.Contains("{"))
                    ++braceCt;
                else if (lowerLine.Trim().Contains("}"))
                {
                    --braceCt;
                    if (braceCt == 0)
                        break;
                }
                else if (lowerLine.Equals("dispinfo"))
                    SkipBraces(ref line, lines); // Eat displacement maps?
                else if (lowerLine.Equals("editor"))
                    SkipBraces(ref line, lines); // Eat editor data?
                else
                {
                    ParseProperty(shortLine, stubs);
                }
            }

            if (stubs.ContainsKey("plane") && stubs.ContainsKey("material") &&
                stubs.ContainsKey("uaxis") && stubs.ContainsKey("vaxis") && stubs.ContainsKey("rotation"))
            {
                Face f = Face.ExplicitFace(new Map.Texturing.Valve220TextureStrategy());
                string[] plane = stubs["plane"].Split(SIDE_SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);
                string[] uaxis = stubs["uaxis"].Split(SIDE_SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);
                string[] vaxis = stubs["vaxis"].Split(SIDE_SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);

                f.PlanePointA = Face.ParseVec3(plane, 0);
                f.PlanePointB = Face.ParseVec3(plane, 3);
                f.PlanePointC = Face.ParseVec3(plane, 6);
                f.TexName = stubs["material"];
                f.XTexVector = Face.ParseVec3(uaxis, 0);
                f.YTexVector = Face.ParseVec3(vaxis, 0);
                f.Offset = new Vector2(float.Parse(uaxis[3]), float.Parse(vaxis[3]));
                f.Scale = new Vector2(float.Parse(uaxis[4]), float.Parse(vaxis[4]));
                
                brush.Faces.Add(f);
            }
        }

        void ParseEntity(BrushMap map, ref int line, List<string> lines)
        {
            int braceCt = 0;
            Entity entity = new Entity();
            map.Entities.Add(entity);
            for (; line < lines.Count; ++line)
            {
                string text = lines[line];
                string shortLine = text.Trim();
                string lowerLine = shortLine.ToLower();
                if (lowerLine.Contains("{"))
                {
                    ++braceCt;
                } 
                else if (lowerLine.Contains("}"))
                {
                    --braceCt;
                    if (braceCt == 0)
                        return;
                }
                else if (lowerLine.Equals("editor"))
                {
                    // Skip Editor data
                    SkipBraces(ref line, lines);
                }
                else if (braceCt == 1)
                {
                    ParseProperty(shortLine, entity.Properties);
                }
            }
        }

        void ParseProperty(string line, Dictionary<string,string> target)
        {
            List<string> terms = new List<string>(line.Split(SPLIT_PROPERTIES, StringSplitOptions.RemoveEmptyEntries));
            for (int i = 0; i < terms.Count; ++i)
            {
                if (String.IsNullOrWhiteSpace(terms[i]))
                {
                    terms.RemoveAt(i);
                    --i;
                }
            }
            if (terms.Count == 2)
                target[terms[0]] = terms[1];
        }

        void SkipBraces(ref int line, List<string> lines)
        {
            int braceCt = 0;
            for (; line < lines.Count; ++line)
            {
                if (lines[line].Trim().Contains("{"))
                    ++braceCt;
                else if (lines[line].Trim().Contains("}"))
                {
                    --braceCt;
                    if (braceCt == 0)
                        return;
                }
            }
        }
    }
}
