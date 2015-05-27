using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace QMapConverter.Util
{
    public static class MathExt
    {
        public static Vector3 VectorMA(Vector3 veca, float scale, Vector3 vecb)
        {
            Vector3 vec = new Vector3();
	        vec.X = veca.X + scale * vecb.X;
	        vec.Y = veca.Y + scale * vecb.Y;
	        vec.Z = veca.Z + scale * vecb.Z;
            return vec;
        }

        public static Vector3 Vector3FromString(string str)
        {
            string[] parts = str.Split(',');
            return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        public static Vector3 Cross(this Vector3 me, Vector3 other)
        {
            return Vector3.Cross(me, other);
        }

        public static Vector3 Subtract(this Vector3 me, Vector3 other)
        {
            return Vector3.Subtract(me, other);
        }

        public static Vector3 Multiply(this Vector3 me, float by)
        {
            return Vector3.Multiply(me, by);
        }

        public static Vector3 Divide(this Vector3 me, float by)
        {
            return Vector3.Divide(me, by);
        }

        public static Vector3 NearestPoint(this Plane3 plane, Vector3 point)
        {
            Vector3 dir = plane.Normal.Multiply(-1);
            float denom = Vector3.Dot(dir, plane.Normal);
            if (denom != 0)
            {
                float t = (-(Vector3.Dot(point, plane.Normal) + plane.Distance)) / denom;
                if (t < 0)
                    return new Vector3();
                Vector3 ret = point;
                ret = Vector3.Add(ret, dir).Multiply(t);
                return ret;
            } 
            else if (plane.GetDistance(point) == 0.0f) // on the plane
            {
                return point;
            }
            return new Vector3();
        }

        public static Vector2 PlaneSpace(this Plane3 plane, Vector3 point)
        {
            return new Vector2(0, 0);
        }

        // Origin point of the plane
        public static Vector3 Origin(this Plane3 plane) {
            return Vector3.Multiply(plane.Normal, plane.Distance);
        }

        //public static Plane3 PlaneFromPoints(Vector3 a, Vector3 b, Vector3 c)
        //{
        //    return PlaneFromPoints(a, b, c);
        //}

        public static Plane3 PlaneFromOrigin(Vector3 origin, Vector3 normal)
        {
            return new Plane3(normal, Vector3.Dot(normal, origin));
        }

        public static Plane3 PlaneFromPoints(params Vector3[] points)
        {
            Vector3 normal = Vector3.Cross(Vector3.Subtract(points[0], points[1]), Vector3.Subtract(points[0], points[2])).Normalized();
            //normal = Vector3.Multiply(normal, -1);
            float distance = -Vector3.Dot(normal, points[0]);
            return new Plane3(normal, distance);
        }

        public static Vector3[] CWSort(params Vector3[] points)
        {
            List<Vector3> ret = new List<Vector3>();
            Vector3 center = new Vector3();
            foreach (Vector3 v in points)
            {
                center = Vector3.Add(center, v);
            }
            center = Vector3.Divide(center, points.Length);

            while (points.Length > 1)
            {
                float value = float.MaxValue;
                int bestIndex = Int32.MaxValue;
                for (int i = 0; i < points.Length; ++i)
                {
                    float dp = Vector3.Dot(center, points[i]);
                    if (dp < value)
                    {
                        value = dp;
                        bestIndex = i;
                    }
                }
                ret.Add(points[bestIndex]);
                Vector3[] newPoints = new Vector3[points.Length - 1];
                for (int i = 0, j = 0; i < points.Length; ++i)
                {
                    if (i == bestIndex)
                        continue;
                    newPoints[j] = points[i];
                    ++j;
                }
                points = newPoints;
            }
            ret.Add(points[0]);

            return ret.ToArray();
        }

        public static bool PlaneIntersection(Plane3 a, Plane3 b, Plane3 c, out Vector3 point)
        {
            point = new Vector3();
            float denom = Vector3.Dot(a.Normal, Vector3.Cross(b.Normal, c.Normal));
            if (denom == 0.0f)
            {
                return false;
            }

            Vector3 n1 = a.Normal;
            Vector3 n2 = b.Normal;
            Vector3 n3 = b.Normal;
            float d1 = a.Distance;
            float d2 = b.Distance;
            float d3 = c.Distance;

            //point = n2.Cross(n3).Multiply(-d1).Subtract(n3.Cross(n1).Multiply(d2)).Subtract(n1.Cross(n2).Multiply(d3).Divide(denom));

            //p = -d1 * ( n2.Cross ( n3 ) ) – d2 * ( n3.Cross ( n1 ) ) – d3 * ( n1.Cross ( n2 ) ) / denom;
            Vector3 pointA = Vector3.Multiply(Vector3.Cross(b.Normal, c.Normal), -a.Distance);
            Vector3 pointB = Vector3.Multiply(Vector3.Cross(c.Normal, a.Normal), b.Distance);
            Vector3 pointC = Vector3.Multiply(Vector3.Cross(a.Normal, b.Normal), c.Distance);
            point = Vector3.Subtract(pointA, pointB);
            point = Vector3.Subtract(point, pointC);
            point = Vector3.Divide(point, denom);
            return true;
        }

        public static bool Intersects(this BoundingBox3 me, BoundingBox3 other)
        {
            // test using SAT (separating axis theorem)
            float lx = Math.Abs(me.Center.X - other.Center.X);
            float sumx = (me.Size.X / 2.0f) + (other.Size.X / 2.0f);

            float ly = Math.Abs(me.Center.Y - other.Center.Y);
            float sumy = (me.Size.Y / 2.0f) + (other.Size.Y / 2.0f);

            float lz = Math.Abs(me.Center.Z - other.Center.Z);
            float sumz = (me.Size.Z / 2.0f) + (other.Size.Z / 2.0f);

            return (lx <= sumx && ly <= sumy && lz <= sumz);
        }

        public static bool Contains(this BoundingBox3 me, BoundingBox3 other)
        {
            return  me.Min.X <= other.Min.X &&
                    me.Min.Y <= other.Min.Y &&
                    me.Min.Z <= other.Min.Z &&
                    me.Max.X >= other.Max.X &&
                    me.Max.Y >= other.Max.Y &&
                    me.Max.Z >= other.Max.Z;
        }

        public static Vector3 Set(this Vector3 me, Vector3 other)
        {
            me.X = other.X;
            me.Y = other.Y;
            me.Z = other.Z;
            return me;
        }

        public static Vector3 Set(this Vector3 me, float x, float y, float z)
        {
            me.X = x;
            me.Y = y;
            me.Z = z;
            return me;
        }

        public static string PlaneToString(this Plane3 plane)
        {
            return string.Format("{0} {1} {2} - {3} {4} {5}", plane.Origin().X, plane.Origin().Y, plane.Origin().Z, plane.Normal.X, plane.Normal.Y, plane.Normal.Z);
        }

        public static Vector3 MaxVector3(Vector3 left, Vector3 right)
        {
            return new Vector3(
                Math.Max(left.X, right.X),
                Math.Max(left.Y, right.Y),
                Math.Max(left.Z, right.Z)
                );
        }

        public static Vector3 MinVector3(Vector3 left, Vector3 right)
        {
            return new Vector3(
                Math.Min(left.X, right.X),
                Math.Min(left.Y, right.Y),
                Math.Min(left.Z, right.Z)
                );
        }
    }
}
