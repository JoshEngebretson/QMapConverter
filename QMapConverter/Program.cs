using Map;
using Map.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace QMapConverter
{
    class Program
    {
        static void WriteUsage()
        {
            Console.WriteLine("QMapConverter.exe - Copyright (C) 2015 JSandusky");
            Console.WriteLine("    Usage:");
            Console.WriteLine("        QMapConverter <mode> <material-dir> <input-file> <output-file> [switches]");
            Console.WriteLine("Possible Modes:");
            Console.WriteLine("    scene - output a complete Urho3D scene");
            Console.WriteLine("    prefab - output an Urho3D prefab");
            Console.WriteLine("    model - output a single Urho3D model");
            Console.WriteLine("    obj - output a single OBJ model");
            Console.WriteLine("Switches:");
            Console.WriteLine("    -celldim [x,y,z:30,30,FloatMax] - block size for mesh clustering");
            Console.WriteLine("    -scale [x,y,z:1,1,1] - multiplier to scale by");
            Console.WriteLine("    -nodetail <bool:false> - ignore detail brushes");
            Console.WriteLine("    -tess <integer:-1> - force patch tesselation level");
            Console.WriteLine("    -vmf <bool:false> - parse as VMF format");
            Console.WriteLine("    -write <format>");
            Console.WriteLine("        q1 - Quake 1");
            Console.WriteLine("        q3 - Quake 2/3");
            Console.WriteLine("        gtk - GtkRadiant brush primitives");
        }

        

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                WriteUsage();
                return;
            }
            string mode = args[0].ToLowerInvariant();

            Settings.SelectedWriter = WriterSelect.SelectWriter(mode);

            Dictionary<string, string> arguments = new Dictionary<string, string>();
            for (int i = 4; i < args.Length;)
            {
                if (args[i].StartsWith("-"))
                {
                    arguments[args[i]] = args[i + 1];
                    i += 2;
                }
                else
                {
                    arguments[args[i]] = args[i];
                    ++i;
                    continue;
                }
            }

            if (!Settings.SelectedWriter.CheckArgs(args, arguments))
            {
                Settings.SelectedWriter.PrintUsage();
                return;
            }

            Settings.ContentDir = args[1];
            string inputFile = args[2];

            Map.BrushMap map = null;
            //try
            //{
                Settings.FillSettings(arguments);

                // Check if this is a VMF file
                if (inputFile.Contains(".vmf"))
                    Settings.SelectedReader = new VmfReader();

                map = Settings.SelectedReader.ReadMap(inputFile, arguments);

                int brushCt = 0;
                foreach (Map.Entity e in map.Entities)
                    brushCt += e.Brushes.Count;
                using (Util.ConsoleProgress prog = new Util.ConsoleProgress("Computing Faces", brushCt))
                {
                    for (int i = 0; i < map.Entities.Count; ++i)
                    {
                        Entity e = map.Entities[i];
                        for (int j = 0; j < e.Brushes.Count; ++j)
                        {
                            Brush b = e.Brushes[j];
                            prog.Increment();
                            b.ComputeFaces();
                            prog.Write();
                            b.Clean();
                            if (b.Faces.Count == 0)
                            {
                                e.Brushes.RemoveAt(j);
                                --j;
                            }
                        }
                        if (e.Brushes.Count == 0 && e.Patches.Count == 0 && e.Properties.Count == 0)
                        {
                            map.Entities.RemoveAt(i);
                            --i;
                            continue;
                        }
                    }
                    prog.Finish();
                }
            //} 
            //catch (Exception ex)
            //{
            //    Console.WriteLine("ERROR:");
            //    Console.WriteLine(ex.Message);
            //    return;
            //}

            Settings.SelectedWriter.WriteMap(map);
        }
    }
}
