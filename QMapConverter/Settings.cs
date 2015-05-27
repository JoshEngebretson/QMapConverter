using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace QMapConverter
{
    public static class Settings
    {
        public static Vector3 MapScale = new Vector3(1f, 1f, 1f);
        public static Vector3 CellSize = new Vector3(300, float.MaxValue, 300);
        public static bool FlipNormals = false;
        public static Map.AbstractMapReader SelectedReader = null;
        public static Map.AbstractMapWriter SelectedWriter = null;
        public static int ForcedPatchLevel = -1;
        public static string MaterialSourceDir = "";
        public static string ContentDir = "";

        public static void FillSettings(Dictionary<string,string> settings)
        {
            if (settings.ContainsKey("scale"))
                MapScale = Util.MathExt.Vector3FromString(settings["scale"]);
            FlipNormals = GetSwitch("flipn", false, settings);

            if (settings.ContainsKey("celldim"))
                CellSize = Util.MathExt.Vector3FromString(settings["celldim"]);

            if (settings.ContainsKey("tess"))
                ForcedPatchLevel = int.Parse(settings["tess"]);

            if (GetSwitch("vmf", false, settings))
                SelectedReader = new Map.Readers.VmfReader();
            else
                SelectedReader = new Map.Readers.QuakeReader();

            if (settings.ContainsKey("write"))
            {
                string writerName = settings["write"];
                if (writerName.ToLowerInvariant().Equals("q1"))
                {
                    SelectedWriter = new Map.Writers.QuakeWriter();
                } 
                else if (writerName.ToLowerInvariant().Equals("q3"))
                {
                    SelectedWriter = new Map.Writers.Quake3Writer();
                } 
                else if (writerName.ToLowerInvariant().Equals("gtk"))
                {
                    SelectedWriter = new Map.Writers.GtkRadiantWriter();
                }
            }
        }

        static bool GetSwitch(string key, bool defaultValue, Dictionary<string, string> settings)
        {
            if (settings.ContainsKey(key))
                return settings[key].Equals("1") || settings[key].ToLower().Equals("true");
            return defaultValue;
        }
    }
}
