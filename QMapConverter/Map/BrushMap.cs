using QMapConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toe;

namespace Map
{    
    public class ImageSize {
        public int Width {get;set;}
        public int Height {get;set;}
    }

    public class BrushMap
    {
        public Dictionary<string, string> SceneProperties = new Dictionary<string, string>();
        public List<Entity> Entities = new List<Entity>();
        public Dictionary<string, ImageSize> ImageSizes = new Dictionary<string, ImageSize>();
        public List<string> KnownTextures = new List<string>();

        public BrushMap() { }

        public void CalculateImageSizes(string lookupDir)
        {
            foreach (Entity e in Entities)
            {
                foreach (Brush b in e.Brushes)
                {
                    foreach (Face f in b.Faces)
                    {
                        if (!KnownTextures.Contains(f.TexName))
                            KnownTextures.Add(f.TexName);
                    }
                }

                foreach (Patch p in e.Patches)
                {
                    if (!KnownTextures.Contains(p.TextureName))
                        KnownTextures.Add(p.TextureName);
                }
            }

            using (QMapConverter.Util.ConsoleProgress prog = new QMapConverter.Util.ConsoleProgress("Building Tex Data", KnownTextures.Count))
            {
                foreach (string tex in KnownTextures)
                {
                    prog.Increment();
                    prog.Write();


                }
            }
        }

        public BoundingBox CalculateBounds()
        {
            BoundingBox ret = new BoundingBox();
            foreach (Entity e in Entities)
            {
                foreach (Brush b in e.Brushes)
                {
                    BoundingBox bnds = b.CalculateBounds();
                    ret.ext(bnds);
                }
            }

            return ret;
        }

        public List<Entity> SliceBack(Plane3 slicePlane)
        {
            List<Entity> back = new List<Entity>();
            Slice(slicePlane, null, back);
            return back;
        }

        public List<Entity> SliceFront(Plane3 slicePlane)
        {
            List<Entity> front = new List<Entity>();
            Slice(slicePlane, front, null);
            return front;
        }

        public void Slice(Plane3 slicePlane, List<Entity> front, List<Entity> back)
        {
            foreach (Entity e in Entities)
                e.Slice(slicePlane, front, back);
        }

        // Slice a given list of entities
        public static void Slice(Plane3 slicePlane, List<Entity> toSlice, List<Entity> front, List<Entity> back)
        {
            foreach (Entity e in toSlice)
            {
                //if (e.Brushes.Count > 0)
                    e.Slice(slicePlane, front, back);
            }
        }

        public static void SliceAndRemove(Plane3 slicePlane, List<Entity> toSlice, List<Entity> front, List<Entity> back)
        {
            for (int i = 0; i < toSlice.Count; ++i)
            {
                if (toSlice[i].Slice(slicePlane, front, back) == SplitResult.Split)
                {
                    toSlice.RemoveAt(i);
                    --i;
                }
            }
        }
    }
}
