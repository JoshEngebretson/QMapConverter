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
        public List<Face> Faces;

        public Brush()
        {
            Faces = new List<Face>();
        }

        public void ComputeFaces()
        {
            for (int i = 0; i < Faces.Count; ++i)
            {
                Face faceA = Faces[i];
                Plane3 planeA = MathExt.PlaneFromPoints(faceA.PlanePointA, faceA.PlanePointB, faceA.PlanePointC);
                for (int j = 0; j < Faces.Count; ++j)
                {
                    Face faceB = Faces[j];
                    Plane3 planeB = MathExt.PlaneFromPoints(faceB.PlanePointA, faceB.PlanePointB, faceB.PlanePointC);
                    for (int k = 0; k < Faces.Count; ++k)
                    {
                        // Unique triplet?
                        if (i != j && i != k && j != k)
                        {
                            Face faceC = Faces[k];
                            Plane3 planeC = MathExt.PlaneFromPoints(faceC.PlanePointA, faceC.PlanePointB, faceC.PlanePointC);
                            Vector3 point = new Vector3();
                            bool legal = true;
                            if (MathExt.PlaneIntersection(planeA, planeB, planeC, out point))
                            {
                                for (int l = 0; l < Faces.Count; ++l)
                                {
                                    if (l == j || l == k || l == i)
                                        continue;
                                    Plane3 testPlane = MathExt.PlaneFromPoints(Faces[l].PlanePointA, Faces[l].PlanePointB, Faces[l].PlanePointC);
                                    float distance = testPlane.GetDistance(point);
                                    if (distance < 0.0f) // Outside of brush?
                                    {
                                        legal = false;
                                    }
                                }
                                if (legal)
                                {
                                    foreach (Vector3 pt in faceA.Vertices)
                                    {
                                        if (Vector3.Subtract(point, pt).Length == 0)
                                            legal = false;
                                    }
                                    //if (!(Vector3.Subtract(point, faceA.PosA).Length == 0 ||
                                    //    Vector3.Subtract(point, faceA.PosB).Length == 0 ||
                                    //    Vector3.Subtract(point, faceA.PosC).Length == 0))
                                    if (legal)
                                        faceA.Vertices.Add(point);
                                        //faceA.PosD = point;
                                }
                            }
                        }
                    }
                }
                faceA.CalculateVertices();
            }
        }
    }
}
