using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map
{
    public enum EntityType
    {
        Brush,
        Patch,
        Mesh
    }

    public class Entity
    {
        public EntityType Kind = EntityType.Brush;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public List<Brush> Brushes = new List<Brush>();
        public List<Patch> Patches = new List<Patch>();

        public Entity() { }
        public Entity(Entity reference)
        {
            Kind = reference.Kind;
            foreach (string key in reference.Properties.Keys)
                Properties[key] = reference.Properties[key];
        }

        public int ContentCount()
        {
            return Patches.Count + Brushes.Count;
        }

        public static int TriangleCount(List<Entity> e)
        {
            int ret = 0;
            foreach (Entity ent in e)
            {
                foreach (Brush b in ent.Brushes)
                    foreach (Face f in b.Faces)
                        ret += f.Vertices.Count - 2;
            }
            return ret;
        }

        public static int Count(List<Entity> e)
        {
            int ret = 0;
            foreach (Entity ent in e)
                ret += ent.ContentCount();
            return ret;
        }

        public Vector3 GetOrigin()
        {
            if (Properties.ContainsKey("origin"))
                return Face.ParseVec3(Properties["origin"].Split(' '), 0);
            return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        }

        public SplitResult Slice(Plane3 plane, List<Entity> frontEntities, List<Entity> backEntities)
        {
            List<Brush> frontBrushes = new List<Brush>();
            List<Brush> backBrushes = new List<Brush>();
            List<Patch> frontPatches = new List<Patch>();
            List<Patch> backPatches = new List<Patch>();

            foreach (Brush b in Brushes)
                b.Slice(plane, frontEntities != null ? frontBrushes : null, backEntities != null ? backBrushes : null);

            int frontSideCt = frontBrushes.Count + frontPatches.Count;
            int backSideCt = backBrushes.Count + backPatches.Count;

            // Simple cases of entirely on front or back side first
            if (backSideCt == 0 && frontSideCt > 0)
            {
                if (frontEntities != null)
                    frontEntities.Add(this);
                return SplitResult.Front;
            }
            else if (frontSideCt == 0 && backSideCt > 0)
            {
                if (backEntities != null)
                    backEntities.Add(this);
                return SplitResult.Back;
            }
            else if (frontSideCt > 0 && backSideCt > 0)
            {
                // Divide this entity into 2 new entities containing the sperate geometries
                Entity frontEntity = new Entity(this);
                Entity backEntity = new Entity(this);
                foreach (Brush b in frontBrushes)
                {
                    b.ComputeFaces();
                    frontEntity.Brushes.Add(b);
                }
                foreach (Brush b in backBrushes)
                {
                    b.ComputeFaces();
                    backEntity.Brushes.Add(b);
                }

                if (frontEntities != null && frontEntity.Brushes.Count > 0)
                    frontEntities.Add(frontEntity);
                if (backEntities != null && backEntity.Brushes.Count > 0)
                    backEntities.Add(backEntity);
                return SplitResult.Split;
            }

            //if (Properties.ContainsKey("origin"))
            //{
            //    if (plane.GetDistance(GetOrigin()) > 0)
            //        frontEntities.Add(this);
            //    else
            //        backEntities.Add(this);
            //}
            return SplitResult.None;
        }

        public static List<Brush> GetBrushes(List<Entity> entities)
        {
            List<Brush> ret = new List<Brush>();
            foreach (Entity e in entities)
                if (e.Brushes.Count > 0)
                    ret.AddRange(e.Brushes);
            return ret;
        }

        public static void CleanOccludedFaces(List<Brush> brushes)
        {
            for (int i = 0; i < brushes.Count; ++i)
            {
                Brush leftBrush = brushes[i];
                for (int j = 0; j < brushes.Count; ++j)
                {
                    if (j == i)
                        continue;
                    Brush rightBrush = brushes[i];

                    for (int leftFaceIdx = 0; leftFaceIdx < leftBrush.Faces.Count; ++leftFaceIdx)
                    {
                        for (int rightFaceIdx = 0; rightFaceIdx < rightBrush.Faces.Count; ++rightFaceIdx)
                        {

                        }
                    }
                }
            }
        }
    }
}
