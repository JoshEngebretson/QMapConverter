using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;
using QMapConverter.Util;
using QMapConverter;

namespace Map
{
    public class BSPCell : List<BSPCell>
    {
        public BSPCell Parent;
        public Plane3 Plane;
        public Vector3 Centroid;
        public List<Entity> FrontEntities = new List<Entity>();
        public List<Entity> BackEntities = new List<Entity>();

        private BSPCell() { }

        public BSPCell(List<Entity> entities, Plane3 plane, BSPCell parent = null)
        {
            Plane = plane;
            Parent = parent;
            foreach (Entity e in entities)
                e.Slice(Plane, FrontEntities, BackEntities);
        }

        // Cross division against the Y axis
        public void DivideEven(BrushMap map, int depth, bool advance, Vector3 advancement)
        {
            // Cross product for our normal
            Vector3 nextPlaneNormal = Vector3.Cross(Plane.Normal, Vector3.UnitY);
            Vector3 planeOrigin = Plane.Origin();
            float distance = planeOrigin.Length;

            if (depth > 0)
            {
                Plane3 frontPlane = new Plane3(nextPlaneNormal, advance ? Vector3.Multiply(advancement, distance).Length : distance);
                BSPCell frontCell = new BSPCell { Plane = frontPlane, Parent = this };
                Add(frontCell);

                Plane3 backPlane = new Plane3(nextPlaneNormal, advance ? Vector3.Multiply(advancement, distance).Length : distance);
                BSPCell backCell = new BSPCell { Plane = backPlane, Parent = this };
                Add(backCell);

                if (advance)
                    advancement = Vector3.Multiply(advancement, 0.5f);

                frontCell.DivideEven(map, depth - 1, !advance, advancement);
                backCell.DivideEven(map, depth - 1, !advance, advancement);
            }

        }

        // Coincident plane division
        public void DivideSurface()
        {

        }
    }

    // Divides space using hyperplanes
    public class BSP
    {
        BrushMap map;

        public BSP(BrushMap map)
        {
            this.map = map;
        }

        // Slice into an even grid
        public BSPCell SliceGrid(int depth)
        {
            Plane3 startPlane = MathExt.PlaneFromOrigin(new Vector3(0, 0, 0), Vector3.UnitX);
            BSPCell ret = new BSPCell(map.Entities, startPlane);

            return ret;
        }

        // Quake style BSP slicing using coincident planes of faces
        public void SliceBSP(int depth)
        {

        }

        public List<List<Entity>> MarchingDivide(Vector3 normal, Vector3 from, Vector3 to, int steps, List<Entity> entities)
        {
            List<List<Entity>> ret = new List<List<Entity>>();

            List<Entity> dividing = new List<Entity>(entities);
            for (int i = 0; i < steps; ++i)
            {
                Vector3 origin = Vector3.Lerp(from, to, ((float)i+1) / ((float)steps));
                Plane3 divPlane = MathExt.PlaneFromOrigin(origin, normal);

                List<Entity> front = new List<Entity>();
                List<Entity> back = new List<Entity>();
                BrushMap.Slice(divPlane, dividing, front, back);

                //if (back.Count > 0)
                    ret.Add(back);
                    dividing = front;
            }
            ret.Add(dividing);
            return ret;
        }

        public void TestDivide()
        {
            BoundingBox mapBounds = map.CalculateBounds();

            // Slice along the X-Axis
            List<List<Entity>> xSliced = MarchingDivide(Vector3.UnitX, 
                new Vector3(mapBounds.min.X, 0, 0), 
                new Vector3(mapBounds.max.X, 0, 0), 4, map.Entities);

            int entitySum = 0;
            foreach (List<Entity> xslice in xSliced)
            {
                List<List<Entity>> zSliced = MarchingDivide(Vector3.UnitZ, 
                    new Vector3(0, 0, mapBounds.min.Z), 
                    new Vector3(0, 0, mapBounds.max.Z), 4, xslice);

                Console.WriteLine("  " + xslice.Count);
                foreach (List<Entity> zslice in zSliced)
                {
                    //List<List<Entity>> ySliced = MarchingDivide(Vector3.UnitY, 
                    //    new Vector3(0, mapBounds.min.Y, 0), 
                    //    new Vector3(0, mapBounds.max.Y, 0), 4, zslice);
                    
                    Console.WriteLine("    " + zslice.Count + " - " + Entity.Count(zslice) + " - " + Entity.TriangleCount(zslice));
                    //foreach (List<Entity> yslice in ySliced)
                    //{
                    //    Console.WriteLine("      " + yslice.Count);
                    //    entitySum += yslice.Count;
                    //}
                }
            }
            Console.WriteLine(String.Format("Total Entity Count: {0} / {1}", entitySum, map.Entities.Count));

        }

        public void GridDivide(BrushMap map, int gridX, int gridY, int gridZ)
        {
            BoundingBox mapBounds = map.CalculateBounds();

            List<Entity> openList = new List<Entity>(map.Entities);
            Console.WriteLine("Executing bsp...");
            // March along the X-Axis
            for (int x = 0; x < gridX; ++x)
            {
                Vector3 xVec = new Vector3(mapBounds.min);
                xVec.X = xVec.X + (mapBounds.Dim.X / gridX) * (x + 1);
                Plane3 xPlane = MathExt.PlaneFromOrigin(xVec, Vector3.UnitX);

                List<Entity> xBackSide = new List<Entity>();
                // Store anything on the frontside in 
                List<Entity> newOpenList = new List<Entity>();
                BrushMap.Slice(xPlane, openList, newOpenList, xBackSide);
                openList = newOpenList;

                Console.WriteLine(String.Format("X-{2}: {0} front, {1} back - {3}", Entity.Count(openList), Entity.Count(xBackSide), x, xPlane.PlaneToString()));

                for (int y = 0; y < gridY; ++y)
                {
                    Vector3 yVec = new Vector3(mapBounds.min);
                    yVec.Y = yVec.Y + (mapBounds.Dim.Y / gridY) * (y + 1);
                    Plane3 yPlane = MathExt.PlaneFromOrigin(yVec, Vector3.UnitY);

                    List<Entity> yBackSide = new List<Entity>();
                    List<Entity> yFrontSide = new List<Entity>();
                    BrushMap.Slice(yPlane, xBackSide, yFrontSide, yBackSide);

                    Console.WriteLine(String.Format("    Y-{2}: {0} front, {1} back - {3}", Entity.Count(yFrontSide), Entity.Count(yBackSide), y, yPlane.PlaneToString()));

                    for (int z = 0; z < gridZ; ++z)
                    {
                        Vector3 zVec = new Vector3(mapBounds.min);
                        zVec.Z = zVec.Z + (mapBounds.Dim.Z / gridZ) * (z + 1);
                        Plane3 zPlane = MathExt.PlaneFromOrigin(zVec, Vector3.UnitZ);

                        List<Entity> backSide = new List<Entity>();
                        List<Entity> frontSide = new List<Entity>();
                        BrushMap.Slice(zPlane, yBackSide, frontSide, backSide);
                        Console.WriteLine(String.Format("        Zb-{2}: {0} front, {1} back - {3}", Entity.Count(frontSide), Entity.Count(backSide), z, zPlane.PlaneToString()));

                        BrushMap.Slice(zPlane, yFrontSide, frontSide, backSide);
                        Console.WriteLine(String.Format("        Zf-{2}: {0} front, {1} back - {3}", Entity.Count(frontSide), Entity.Count(backSide), z, zPlane.PlaneToString()));

                        // Remove sliced entities
                        //foreach (Entity e in backSide)
                        //    openList.Remove(e);
                        //if (z == gridZ - 1) // Last one, do both sides
                        //    foreach (Entity e in frontSide)
                        //        openList.Remove(e);
                    }
                }
            }
        }

        public void Divide(BrushMap map, int depth)
        {
            // Slice into quads
            Plane3 xPlane = MathExt.PlaneFromOrigin(new Vector3(0,0,0), Vector3.UnitX);
            List<Entity> xFront = new List<Entity>();
            List<Entity> xBack = new List<Entity>();
            map.Slice(xPlane, xFront, xBack);

            Plane3 yPlane = MathExt.PlaneFromOrigin(new Vector3(0, 0, 0), Vector3.UnitY);
            List<Entity> yFront = new List<Entity>(xFront);
            List<Entity> yBack = new List<Entity>(xBack);
            BrushMap.Slice(yPlane, xBack, yFront, yBack);
            BrushMap.Slice(yPlane, xFront, yFront, yBack);

            Plane3 zPlane = MathExt.PlaneFromOrigin(new Vector3(0, 0, 0), Vector3.UnitZ);
            List<Entity> zFront = new List<Entity>(yFront);
            List<Entity> zBack = new List<Entity>(yBack);
            BrushMap.Slice(zPlane, yFront, zFront, zBack);
            BrushMap.Slice(zPlane, yBack, zFront, zBack);
        }
    }
}
