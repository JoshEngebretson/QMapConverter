using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map
{
    public class PatchVert
    {
        public Vector3 Position;
        public Vector3 Color = new Vector3(1, 1, 1);
        public float Alpha = 1.0f;
        public Vector2 UV;
    }

    public class Patch
    {
        public bool LiteralMesh; // true = no bezier interpolation (terrain), false = use bezier interpolation (curve)
        public int Rows = -1;
        public int Columns = -1;
        public string TextureName = "";
        public List<List<PatchVert>> PatchVertices = new List<List<PatchVert>>();

        public List<PatchVert> FlatVertices = new List<PatchVert>();

        public void ArrangeVertices()
        {
            if (FlatVertices.Count > 0 && PatchVertices.Count == 0)
            {
                for (int i = 0; i < Rows; ++i)
                {
                    PatchVertices.Add(new List<PatchVert>());
                    for (int j = 0; j < Columns; ++i)
                    {
                        PatchVertices[Rows].Add(FlatVertices[0]);
                        FlatVertices.RemoveAt(0);
                    }
                }
            }
        }

        public List<Vector3> vertex = new List<Vector3>();
        public List<int> index = new List<int>();
        public List<Vector2> uvs = new List<Vector2>();

        // Calculate UVs for our tessellated vertices 
        private Vector2 BezCurveUV(float t, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            Vector2 bezPoint = new Vector2();

            float a = 1f - t;
            float tt = t * t;

            float[] tPoints = new float[2];
            for (int i = 0; i < 2; i++)
            {
                tPoints[i] = ((a * a) * p0[i]) + (2 * a) * (t * p1[i]) + (tt * p2[i]);
            }

            bezPoint = new Vector2(tPoints[0], tPoints[1]);

            return bezPoint;
        }

        // Calculate a vector3 at point t on a bezier curve between
        // p0 and p2 via p1.  
        private Vector3 BezCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vector3 bezPoint = new Vector3();

            float a = 1f - t;
            float tt = t * t;

            float[] tPoints = new float[3];
            for (int i = 0; i < 3; i++)
            {
                tPoints[i] = ((a * a) * p0[i]) + (2 * a) * (t * p1[i]) + (tt * p2[i]);
            }

            bezPoint = new Vector3(tPoints[0], tPoints[1], tPoints[2]);

            return bezPoint;
        }

        // This takes a tessellation level and three vector3
        // p0 is start, p1 is the midpoint, p2 is the endpoint
        // The returned list begins with p0, ends with p2, with
        // the tessellated verts in between.
        private List<Vector3> Tessellate(int level, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            List<Vector3> vects = new List<Vector3>();

            float stepDelta = 1.0f / level;
            float step = stepDelta;

            vects.Add(p0);
            for (int i = 0; i < (level - 1); i++)
            {
                vects.Add(BezCurve(step, p0, p1, p2));
                step += stepDelta;
            }
            vects.Add(p2);
            return vects;
        }

        // Same as above, but for UVs
        private List<Vector2> TessellateUV(int level, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            List<Vector2> vects = new List<Vector2>();

            float stepDelta = 1.0f / level;
            float step = stepDelta;

            vects.Add(p0);
            for (int i = 0; i < (level - 1); i++)
            {
                vects.Add(BezCurveUV(step, p0, p1, p2));
                step += stepDelta;
            }
            vects.Add(p2);
            return vects;
        }

        // Where the magic happens.
        public Patch(bool isMesh)
        {
            LiteralMesh = isMesh;
        }

        public void Generate(int level)
        {
            vertex.Clear();
            index.Clear();
            List<Vector3> posControl = new List<Vector3>();
            for (int i = 0; i < FlatVertices.Count; ++i)
            {
                posControl.Add(FlatVertices[i].Position);
            }
            List<Vector2> controlUvs = new List<Vector2>();
            for (int i = 0; i < FlatVertices.Count; ++i)
            {
                controlUvs.Add(FlatVertices[i].UV);
            }

            Generate(level, posControl, controlUvs);
        }

        public void BuildIndices()
        {
            vertex.Clear();
            index.Clear();
            int idx = 0;
			ushort pitch = (ushort)(Columns + 1);
			ushort i1 = 0;
			ushort i2 = 1;
			ushort i3 = (ushort)(1 + pitch);
			ushort i4 = pitch;

			ushort row = 0;

			for (int z = 0; z < Rows; z++) {
				for (int x = 0; x < Columns; x++) {
					index[idx++] = i1;
					index[idx++] = i2;
					index[idx++] = i3;

					index[idx++] = i3;
					index[idx++] = i4;
					index[idx++] = i1;

					i1++;
					i2++;
					i3++;
					i4++;
				}

				row += pitch;
				i1 = row;
				i2 = (ushort)(row + 1);
				i3 = (ushort)(i2 + pitch);
				i4 = (ushort)(row + pitch);
			}
        }

        void Generate(int level, List<Vector3> control, List<Vector2> controlUvs)
        {
            // The incoming list is 9 entires, 
            // referenced as p0 through p8 here.

            // Generate extra rows to tessellate
            // each row is three control points
            // start, curve, end
            // The "lines" go as such
            // p0s from p0 to p3 to p6 ''
            // p1s from p1 p4 p7
            // p2s from p2 p5 p8

            List<Vector2> p0suv;
            List<Vector3> p0s;
            p0s = Tessellate(level, control[0], control[3], control[6]);
            p0suv = TessellateUV(level, controlUvs[0], controlUvs[3], controlUvs[6]);

            List<Vector2> p1suv;
            List<Vector3> p1s;
            p1s = Tessellate(level, control[1], control[4], control[7]);
            p1suv = TessellateUV(level, controlUvs[1], controlUvs[4], controlUvs[7]);

            List<Vector2> p2suv;
            List<Vector3> p2s;
            p2s = Tessellate(level, control[2], control[5], control[8]);
            p2suv = TessellateUV(level, controlUvs[2], controlUvs[5], controlUvs[8]);

            // Tessellate all those new sets of control points and pack
            // all the results into our vertex array, which we'll return.
            // Make our uvs list while we're at it.
            for (int i = 0; i <= level; i++)
            {
                vertex.AddRange(Tessellate(level, p0s[i], p1s[i], p2s[i]));
                uvs.AddRange(TessellateUV(level, p0suv[i], p1suv[i], p2suv[i]));
            }

            // This will produce (tessellationLevel + 1)^2 verts
            int numVerts = (level + 1) * (level + 1);

            // Computer triangle indexes for forming a mesh.
            // The mesh will be tessellationlevel + 1 verts
            // wide and tall.
            int xStep = 1;
            int width = level + 1;
            for (int i = 0; i < numVerts - width; i++)
            {
                //on left edge
                if (xStep == 1)
                {
                    index.Add(i);
                    index.Add(i + width);
                    index.Add(i + 1);

                    xStep++;
                    continue;
                }
                else if (xStep == width) //on right edge
                {
                    index.Add(i);
                    index.Add(i + (width - 1));
                    index.Add(i + width);

                    xStep = 1;
                    continue;
                }
                else // not on an edge, so add two
                {
                    index.Add(i);
                    index.Add(i + (width - 1));
                    index.Add(i + width);


                    index.Add(i);
                    index.Add(i + width);
                    index.Add(i + 1);

                    xStep++;
                    continue;
                }
            }
        }

    }
}
