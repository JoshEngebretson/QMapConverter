using QMapConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map
{
    public class Winding : List<Vector3>
    {
        const float BOGUS_RANGE = 8192 * 6;
        const float ON_EPSILON = 0.1f;
        const float SNAP_EPSILON = 0.01f;
        const int MAX_POINTS_ON_WINDING = 64;

        const int SIDE_FRONT = 0;
        const int SIDE_ON = 2;
        const int SIDE_BACK	= 1;

        List<ushort> indices = new List<ushort>();
        Vector3 Origin;

        public Winding(Plane3 fromPlane)
        {
            
            Vector3 normal = fromPlane.Normal;
            float dist = fromPlane.Distance;

            float max, v;
            Vector3 org, vright, vup;

            /* find the major axis */
            max = -BOGUS_RANGE;
            int x = -1;
            for (int i = 0; i < 3; i++)
            {
                v = (i == 0 ? Math.Abs(normal.X) : i == 1 ? Math.Abs(normal.Y) : Math.Abs(normal.Z));
                if (v > max)
                {
                    x = i;
                    max = v;
                }
            }
            if (x == -1)
                throw new Exception("BaseWindingForPlane: no axis found");

            vup = Origin;
            /* axis */
            switch (x)
            {
                case 0:
                case 1:
                    vup[2] = 1;
                    break;
                case 2:
                    vup[0] = 1;
                    break;
            }

            v = Vector3.Dot(vup, normal);
            vup = QMapConverter.Util.MathExt.VectorMA(vup, -v, normal);
            vup.Normalize();

            org = Vector3.Multiply(normal, dist);

            vright = Vector3.Cross(vup, normal);

            vup = Vector3.Multiply(vup, BOGUS_RANGE);
            vright = Vector3.Multiply(vright, BOGUS_RANGE);

            Vector3 vert = Vector3.Subtract(org, vright);
            vert = Vector3.Add(vert, vup);
            Add(vert);

            vert = new Vector3();
            vert = Vector3.Add(org, vright);
            vert = Vector3.Add(vert, vup);
            Add(vert);

            vert = new Vector3();
            vert = Vector3.Add(org, vright);
            vert = Vector3.Subtract(vert, vup);
            Add(vert);

            vert = new Vector3();
            vert = Vector3.Subtract(org, vright);
            vert = Vector3.Subtract(vert, vup);
            Add(vert);
        }

        public void Set(Winding other)
        {
            Clear();
            foreach (Vector3 v in other)
                Add(new Vector3(v));
        }

        public Winding(Winding src)
        {
            Origin = src.Origin;
            for (int i = 0; i < src.Count; ++i)
                Add(new Vector3(src[i]));
        }

        public Winding(Vector3 Origin)
        {
            this.Origin = Origin;
        }

        public Winding ReverseWinding()
        {
            Winding ret = new Winding(this);
            for (int i = 0; i < Count; i++)
                Add(new Vector3(this[Count - 1 - i]));
            return ret;
        }

        public Winding Clip(Plane3 clipPlane, List<Winding> front = null, List<Winding> back = null)
        {
            return Clip(clipPlane, ON_EPSILON, front, back);
        }

        Winding Clip(Plane3 plane, float epsilon, List<Winding> frontFaces, List<Winding> backFaces)
        {
            Vector3 normal = plane.Normal;
            float dist = plane.Distance;
            Vector3 counts = new Vector3();

            List<float> dists = new List<float>();
            List<int> sides = new List<int>();

            int i = 0;
            for (i = 0; i < MAX_POINTS_ON_WINDING + 4; ++i)
            {
                dists.Add(float.NaN);
                sides.Add(-1);
            }

            /* determine sides for each point */
	        for (i = 0; i < Count; i++) {
		        float dot = Vector3.Dot(this[i], normal) - dist;
		        dists[i] = dot;
		        if (dot > epsilon)
			        sides[i] = SIDE_FRONT;
		        else if (dot < -epsilon)
			        sides[i] = SIDE_BACK;
		        else
			        sides[i] = SIDE_ON;
		        counts[sides[i]]++;
	        }
	        sides[i] = sides[0];
	        dists[i] = dists[0];


            Winding front = new Winding(Origin);
            Winding back = new Winding(Origin);

            // All points on backside
	        if (counts[0] == 0.0f) {
                back = new Winding(this);
		        //*front = nullptr;
                if (backFaces != null)
                    backFaces.Add(back);
		        return back;
	        }
            // All points on front side
	        if (counts[1] == 0.0f) {
                front = new Winding(this);
		        //*back = nullptr;
                if (frontFaces != null)
                    frontFaces.Add(front);
		        return front;
	        }

	        /* can't use counts[0] + 2 because of floating point grouping errors */
	        int maxpts = Count + 4;

	        for (i = 0; i < Count; i++) {
		        Vector3 p1 = this[i];
		        Vector3 p2 = new Vector3();
		        float dot;
		        Vector3 mid = new Vector3();

		        if (sides[i] == SIDE_ON) {
                    front.Add(new Vector3(this[i]));
			        back.Add(new Vector3(this[i]));
			        continue;
		        }

		        if (sides[i] == SIDE_FRONT) {
                    front.Add(new Vector3(this[i]));
		        }
		        if (sides[i] == SIDE_BACK) {
                    back.Add(new Vector3(this[i]));
		        }

		        if (sides[i + 1] == SIDE_ON || sides[i + 1] == sides[i])
			        continue;

		        /* generate a split point */
		        p2 = this[(i + 1) % Count];

		        dot = dists[i] / (dists[i] - dists[i + 1]);
		        /* avoid round off error when possible */
		        for (int j = 0; j < 3; j++) {
			        if (normal[j] == 1)
				        mid[j] = dist;
			        else if (normal[j] == -1)
				        mid[j] = -dist;
			        else
				        mid[j] = p1[j] + dot * (p2[j] - p1[j]);
		        }

                front.Add(new Vector3(mid));
                back.Add(new Vector3(mid));
	        }

	        if (front.Count > maxpts || back.Count > maxpts)
		        throw new Exception("ClipWinding: points exceeded estimate");
	        if (front.Count > MAX_POINTS_ON_WINDING || back.Count > MAX_POINTS_ON_WINDING)
		        throw new Exception("ClipWinding: MAX_POINTS_ON_WINDING");

            if (frontFaces != null)
                frontFaces.Add(front);
            if (backFaces != null)
                backFaces.Add(back);
            return front;
        }

        public float Area
        {
            get
            {
                int i;
                Vector3 d1, d2, cross;
                float total;

                total = 0;
                for (i = 2; i < Count; i++)
                {
                    d1 = Vector3.Subtract(this[i - 1], this[0]);
                    d2 = Vector3.Subtract(this[i], this[0]);
                    cross = Vector3.Cross(d1, d2);
                    total += cross.Length;
                }
                return total * 0.5f;
            }
        }

        public Vector3 Center
        {
            get
            {
                Vector3 center = Origin;

                for (int i = 0; i < Count; i++)
                    center = Vector3.Add(this[i], center);

                float scale = 1.0f / ((float)Count);
                return Vector3.Multiply(center, scale);
            }
        }

        public BoundingBox Bounds
        {
            get
            {
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                foreach (Vector3 v in this)
                {
                    max.X = Math.Max(max.X, v.X);
                    max.Y = Math.Max(max.Y, v.Y);
                    max.Z = Math.Max(max.Z, v.Z);

                    min.X = Math.Min(min.X, v.X);
                    min.Y = Math.Min(min.Y, v.Y);
                    min.Z = Math.Min(min.Z, v.Z);
                }

                return new BoundingBox(min, max);
            }
        }

        public List<ushort> Indices { get {return indices; } }

        static Vector3 SnapWeldVector(Vector3 a, Vector3 b)
        {
            Vector3 ret = new Vector3();
            /* do each element */
	        for (int i = 0; i < 3; i++) {
		        /* round to integer */
		        float ai = Math.Abs(a[i]);
		        float bi = Math.Abs(a[i]);

		        /* prefer exact integer */
		        if (ai == a[i])
			        ret[i] = a[i];
		        else if (bi == b[i])
			        ret[i] = b[i];

		        /* use nearest */
		        else if (Math.Abs(ai - a[i]) < Math.Abs(bi < b[i] ? 1.0f : 0.0f))
			        ret[i] = a[i];
		        else
			        ret[i] = b[i];

		        /* snap */
		        float outi = Math.Abs(ret[i]);
		        if (Math.Abs(outi - ret[i]) <= SNAP_EPSILON)
			        ret[i] = outi;
	        }
            return ret;
        }

        public bool Fix()
        {
            bool valid = true;

            /* check all verts */
            for (int i = 0; i < Count; i++)
            {
                /* get second point index */
                int j = (i + 1) % Count;
                Vector3 vec = new Vector3();

                /* don't remove points if winding is a triangle */
                if (Count == 3)
                    return valid;

                /* degenerate edge? */
                vec = Vector3.Subtract(this[i], this[j]);
                float dist = vec.Length;
                if (dist < ON_EPSILON)
                {
                    valid = false;

                    RemoveAt(i);
                    --i;
                    /* create an average point (ydnar 2002-01-26: using nearest-integer weld preference) */
                    //vec = SnapWeldVector(this[i].Position, this[j].Position);
                    ////this[i].Position = vec;
                    //
                    ///* move the remaining verts */
                    //for (int k = i + 2; k < Count; k++)
                    //    this[k - 1] = this[k];
                }
            }

            /* one last check and return */
            if (Count < 3)
                valid = false;
            return valid;
        }

        public Plane3 GetPlane()
        {
            if (this.Count >= 3)
                return QMapConverter.Util.MathExt.PlaneFromPoints(this[0], this[1], this[2]);
            return new Plane3();
        }
    }
}
