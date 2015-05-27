using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;
using QMapConverter.Util;
using QMapConverter;

namespace Map.Sorting
{
    public class Bucket : List<object>
    {
        public BoundingBox Bounds;

        public int GeoCount()
        {
            List<Brush> toConsider = new List<Brush>();
            foreach (object o in this)
            {
                if (o is Brush)
                    toConsider.Add(((Brush)o));
            }
            return TexSort.FacesByTexture(toConsider).Count();
        }
    }

    public class SpaceSort
    {
        BoundingBox bounds;
        Vector3 halfSize;
        Vector3 cellSize;
        Bucket[, ,] Buckets;
        int dimX;
        int dimY;
        int dimZ;

        public SpaceSort(BrushMap map)
        {
            Vector3 cellSize = QMapConverter.Settings.CellSize;
            this.cellSize = cellSize;
            bounds = map.CalculateBounds();

            dimX = 3;//Math.Max(((int)Math.Ceiling(bounds.Dim.X)) / ((int)cellSize.X), 1);
            dimY = 1;//Math.Max(((int)Math.Ceiling(bounds.Dim.Y)) / ((int)cellSize.Y), 1);
            dimZ = 9;//Math.Max(((int)Math.Ceiling(bounds.Dim.Z)) / ((int)cellSize.Z), 1);

            Console.WriteLine(String.Format("Cluster Size: {0} x {1} x {2}", dimX, dimY, dimZ));
            if ((dimX * dimZ * dimY) > 100)
            {
                Console.WriteLine(String.Format("{0} is too many clusters, this will take a while", dimX * dimY * dimZ));
            }

            halfSize = new Vector3(bounds.Dim.X *0.5f,
                bounds.Dim.Y * 0.5f,
                bounds.Dim.Z * 0.5f);

            Buckets = new Bucket[dimX,dimY,dimZ];

            Vector3 reachEnd = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 atMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            for (int x = 0; x < dimX; ++x)
            {
                float xPos = bounds.min.X + (x * bounds.Dim.X / dimX);
                for (int y = 0; y < dimY; ++y)
                {
                    float yPos = bounds.min.Y + (y * bounds.Dim.Y / dimY);
                    for (int z = 0; z < dimZ; ++z)
                    {
                        float zPos = bounds.min.Z + (z * bounds.Dim.Z / dimZ);

                        Vector3 min = new Vector3(xPos, yPos, zPos);
                        Vector3 max = new Vector3(
                            xPos + bounds.Dim.X/dimX, 
                            yPos + bounds.Dim.Y/dimY, 
                            zPos + bounds.Dim.Z/dimZ);

                        atMin = MathExt.MinVector3(min, atMin);
                        reachEnd = MathExt.MaxVector3(max, reachEnd);
                        BoundingBox bnds = new BoundingBox(min, max);
                        Buckets[x, y, z] = new Bucket { Bounds = bnds };
                    }
                }
            }

            foreach (Entity e in map.Entities)
            {
                foreach (Brush b in e.Brushes)
                {
                    // Only insert brushes with faces
                    if (b.Faces.Count > 0)
                        Insert(b, null);
                }
                //foreach (Patch p in e.Patches)
                //{
                //    Insert(null, p);
                //}
            }

            for (int x = 0; x < dimX; ++x) {            
                Console.Write("{\n");
                for (int y = 0; y < dimY; ++y)
                {
                    Console.Write("[");
                    for (int z = 0; z < dimZ; ++z)
                        Console.Write(Buckets[x, y, z].GeoCount() + " ");
                    Console.Write("]\n");
                }
                Console.Write("}\n");
            }
        }

        public void Insert(Brush brush, Patch patch)
        {
            BoundingBox brushBnds = brush != null ? brush.CalculateBounds() : patch.CalculateBounds();
            if (brushBnds.min.X < bounds.min.X)
                throw new Exception("Error");
            if (brushBnds.min.Y < bounds.min.Y)
                throw new Exception("Error");
            if (brushBnds.min.Z < bounds.min.Z)
                throw new Exception("Error");

            if (brushBnds.max.X > bounds.max.X)
                throw new Exception("Error");
            if (brushBnds.max.Y > bounds.max.Y)
                throw new Exception("Error");
            if (brushBnds.max.Z > bounds.max.Z)
                throw new Exception("Error");

            Vector3 index = Vector3.Add(brushBnds.Center, halfSize);
            if (index.X < 0 || index.Y < 0 || index.Z < 0)
                throw new Exception("Index error");

            int x = (int)(((int)(brushBnds.Center.X + halfSize.X)) % Math.Max(dimX,1));
            int y = (int)(((int)(brushBnds.Center.Y + halfSize.Y)) % Math.Max(dimY,1));
            int z = (int)(((int)(brushBnds.Center.Z + halfSize.Z)) % Math.Max(dimZ,1));

            // If the bounds is completely contained then just accept
            if (Buckets[x,y,z].Bounds.contains(brushBnds))
            {
                Buckets[x, y, z].Add(brush);
                return;
            }

            // If the bounds is not completely contained, then we'll go into the cell nearest to our center
            float dist = float.MaxValue;
            Bucket best = null;
            Vector3 brushCenter = brushBnds.Center;

            for (int xx = 0; xx < Buckets.GetLength(0); ++xx) {
                for (int yy = 0; yy < Buckets.GetLength(1); ++yy) {
                    for (int zz = 0; zz < Buckets.GetLength(2); ++zz) {
                        //if (Buckets[xx, yy, zz].Bounds.intersects(brushBnds))
                        {
                            Vector3 cellCenter = Buckets[xx, yy, zz].Bounds.Center;
                            float dst = Vector3.Subtract(brushCenter, cellCenter).Length;
                            if (dst < dist)
                            {
                                dist = dst;
                                best = Buckets[xx, yy, zz];
                            }
                        }
                    }
                }
            }
            //if (best != null)
                best.Add(brush);
        }

        public List<Bucket> GetBuckets()
        {
            List<Bucket> ret = new List<Bucket>();
            foreach (Bucket b in Buckets)
                ret.Add(b);
            return ret;
        }
    }
}
