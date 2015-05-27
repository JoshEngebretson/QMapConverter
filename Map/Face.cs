using QMapConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;
using QMapConverter.Util;

namespace Map
{
    public class Face
    {
        AbstractTextureStrategy TextureStrategy;

        public static Vector2 ParseVec2(string[] terms, int startIdx)
        {
            Vector2 ret = new Vector2();
            ret.X = float.Parse(terms[startIdx]);
            ret.Y = float.Parse(terms[startIdx + 1]);
            return ret;
        }

        public static Vector3 ParseVec3(string[] terms, int startIdx)
        {
            Vector3 ret = new Vector3();
            ret.X = float.Parse(terms[startIdx]);
            ret.Y = float.Parse(terms[startIdx + 2]); // Flip Z and Y axes during load
            ret.Z = float.Parse(terms[startIdx + 1]);
            return ret;
        }

        static readonly char[] SPLIT_CHARS = { '(', ' ', ')', '[', ']' };

        public Vector3 PlanePointA;
        public Vector3 PlanePointB;
        public Vector3 PlanePointC;

        public string TexName;

        public Vector2? ImageBasis; // How many units to tile
        public Vector2 Offset;
        public float Angle;
        public Vector2 Scale;

        public Matrix3? TextureMatrix;
        public Vector3? XTexVector;
        public Vector3? YTexVector;

        public List<Vector3> Vertices = new List<Vector3>();
        public List<short> Indices = new List<short>();

        public Face(string lineData, Vector3 scale)
        {
            int brushPrimStyle = lineData.Count(c => c == '(');

            string[] terms = lineData.Split(SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);

            // Read and scale the plane points
            PlanePointA = ParseVec3(terms, 0);
            PlanePointA = Vector3.Multiply(PlanePointA, scale.X);

            PlanePointB = ParseVec3(terms, 3);
            PlanePointB = Vector3.Multiply(PlanePointB, scale.Y);
            
            PlanePointC = ParseVec3(terms, 6);
            PlanePointC = Vector3.Multiply(PlanePointC, scale.Z);

            // GtkRadiant (Vertex) (Vertex) (Vertex) ((TexMatRow) (TextMatRow)) Texture
            if (brushPrimStyle > 6)
            {
                Vector3 texMatTopRow = ParseVec3(terms, 9);
                Vector3 texMatMidRow = ParseVec3(terms, 12);
                Vector3 texMatBotRow = new Vector3(0,0,1);
                TexName = terms[15];
                TextureStrategy = new Texturing.GtkTextureStrategy();
            }
            // Legacy formats (Valve 220, Treyarch, Id)
            else
            {
                int valveStyle = lineData.Count(c => c == '['); // Is this a Valve 220 map?
                
                // Is this a Treyarch style map?
                int stringCt = 0;
                foreach (string s in terms)
                    stringCt += s.Any(c => Char.IsLetter(c)) ? 1 : 0;

                TexName = terms[9];

                Vertices.Add(PlanePointA);
                Vertices.Add(PlanePointB);
                Vertices.Add(PlanePointC);

                // Valve 220
                if (valveStyle > 0)
                {
                    XTexVector = ParseVec3(terms, 10);
                    YTexVector = ParseVec3(terms, 14);
                    Offset = new Vector2(float.Parse(terms[13]), float.Parse(terms[17]));
                    Angle = float.Parse(terms[18]);
                    Scale = ParseVec2(terms, 19);
                    TextureStrategy = new Texturing.Valve220TextureStrategy();
                }
                // Treyarch, defined world-unit \todo
                else if (stringCt == 2)
                {
                    ImageBasis = ParseVec2(terms, 10);
                    Offset = ParseVec2(terms, 12);
                    Angle = float.Parse(terms[14]);
                    TextureStrategy = new Texturing.Quake3TextureStrategy();
                }
                // Id
                else
                {
                    Offset = ParseVec2(terms, 10);
                    Angle = float.Parse(terms[12]);
                    Scale = ParseVec2(terms, 13);
                    TextureStrategy = new Texturing.QuakeTextureStrategy();
                }
            }
        }

        public static int CountFaces(List<Face> faces)
        {
            int ret = 0;
            foreach (Face f in faces)
                ret += f.Vertices.Count - 2;
            return ret;
        }

        public static void GetBounds(List<Face> faces, out Vector3 MinCoords, out Vector3 MaxCoords)
        {
            MinCoords = new Vector3(0,0,0);
            MaxCoords = new Vector3(0,0,0);
            foreach (Face face in faces)
            {
                foreach (Vector3 vec in face.Vertices)
                {
                    MaxCoords.X = Math.Max(MaxCoords.X, vec.X);
                    MaxCoords.Y = Math.Max(MaxCoords.Y, vec.Y);
                    MaxCoords.Z = Math.Max(MaxCoords.Z, vec.Z);
                    MinCoords.X = Math.Min(MinCoords.X, vec.X);
                    MinCoords.Y = Math.Min(MinCoords.Y, vec.Y);
                    MinCoords.Z = Math.Min(MinCoords.Z, vec.Z);
                }
            }
        }

        public static uint GetVertexCount(List<Face> faces)
        {
            uint ret = 0;
            foreach (Face f in faces)
            {
                ret += (uint)f.Vertices.Count;
            }
            return ret;
        }

        public Vector3 GetNormal()
        {
            Plane3 plane = MathExt.PlaneFromPoints(PlanePointA, PlanePointB, PlanePointC);
            return plane.Normal;
        }

        public Vector2 GetTexCoord(Vector3 pos)
        {
            return TextureStrategy.GetTextureCoordinate(this, pos);
        }

        public void CalculateVertices()
        {
            Indices = new List<short>();
            QMapConverter.Util.EarClippingTriangulator clipper = new QMapConverter.Util.EarClippingTriangulator(GetNormal());
            Indices = clipper.computeTriangles(Vertices.ToArray());
        }
    }
}
