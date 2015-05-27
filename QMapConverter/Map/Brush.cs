using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;
using QMapConverter;
using QMapConverter.Util;

namespace Map
{
    public class Brush
    {
        BoundingBox Bounds;
        public List<Face> Faces;
        bool recalced = false;

        public Brush()
        {
            Faces = new List<Face>();
        }

        public void Clean()
        {
            for (int i = 0; i < Faces.Count; ++i)
            {
                if (false)//Faces[i].TexName.ToLowerInvariant().Equals("common/caulk"))// ||
                    //Faces[i].TexName.ToLowerInvariant().Equals("common/nodrawnonsolid") ||
                    //Faces[i].TexName.ToLowerInvariant().Equals("common/weapclip") ||
                    //Faces[i].TexName.ToLowerInvariant().Equals("common/clip") ||
                    //Faces[i].TexName.ToLowerInvariant().Equals("common/skip") ||
                    //Faces[i].TexName.ToLowerInvariant().Equals("common/hint") ||
                    //Faces[i].TexName.ToLowerInvariant().Equals("common/noimpact") ||
                    //Faces[i].TexName.ToLowerInvariant().Equals("common/trigger") || // Should this be detected?
                    //Faces[i].TexName.ToLowerInvariant().Equals("common/warpzone")) // Should this be detected?
                {
                    Faces.RemoveAt(i);
                    --i;
                    continue;
                }
            }
        }

        public void ComputeFaces()
        {
            for (int i = 0; i < Faces.Count; ++i)
            {
                Winding winding = new Winding(Faces[i].GetPlane());

                for (int j = 0; j < Faces.Count; ++j)
                {
                    if (j == i)
                        continue;
                    Winding result = winding.Clip(Faces[j].GetPlane());
                    if (result != null)
                        winding.Set(result);
                }
                if (winding.Fix())
                {
                    foreach (Vector3 v in winding)
                    {
                        Faces[i].Vertices.Add(v);
                    }
                    Faces[i].CalculateIndices();
                }
                else
                {
                    throw new Exception("Unable to fix winding");
                }
            }
        }

        public BoundingBox CalculateBounds()
        {
            recalced = false;
            if (Bounds == null)
            {
                Bounds = Face.GetBounds(Faces);
                recalced = true;
            }
            return Bounds;
        }

        // Slice the brush by a plane
        // If divided by the plane, results will be placed into the lists based on which side of th eplane they're on
        public SplitResult Slice(Plane3 plane, List<Brush> front, List<Brush> back)
        {
            List<Face> frontFaces = new List<Face>();
            List<Face> backFaces = new List<Face>();
            List<Winding> frontWinding = new List<Winding>();
            List<Winding> backWinding = new List<Winding>();
            
            foreach (Face f in Faces)
            {
                frontFaces.Clear();
                backFaces.Clear();
                
                Winding winding = new Winding(f.GetPlane());
                winding.Clip(plane, frontWinding, backWinding);

                for (int i = 0; i < frontWinding.Count; ++i)
                {
                    if (frontWinding[i].Count == 0)
                    {
                        frontWinding.RemoveAt(i);
                        --i;
                    }
                }
                for (int i = 0; i < backWinding.Count; ++i)
                {
                    if (backWinding[i].Count == 0)
                    {
                        backWinding.RemoveAt(i);
                        --i;
                    }
                }

                // Brush needs to be split
                if (frontWinding.Count > 0 && backWinding.Count > 0)
                {
                    Brush frontBrush = new Brush();
                    Brush backBrush = new Brush();

                    if (front != null)
                    {
                        foreach (Winding w in frontWinding)
                            if (w.Count > 0)
                                frontBrush.Faces.Add(new Face(f, w));
                        if (frontBrush.Faces.Count > 0)
                            front.Add(frontBrush);
                    }
                    if (back != null)
                    {
                        foreach (Winding w in backWinding)
                        {
                            if (w.Count > 0)
                                backBrush.Faces.Add(new Face(f, w));
                        }
                        if (backBrush.Faces.Count > 0)
                            back.Add(backBrush);
                    }

                    return SplitResult.Split;
                }
                // Brush is whole
                else if (frontWinding.Count > 0)
                {
                    if (front != null)
                        front.Add(this);
                    return SplitResult.Front;
                }
                else if (backWinding.Count > 0)
                {
                    if (back != null)
                        back.Add(this);
                    return SplitResult.Back;
                }
                else
                    throw new Exception("Unexpected failure with brush neither on nor on a side of a plane");
            }
            return SplitResult.None;
        }
    }
}
