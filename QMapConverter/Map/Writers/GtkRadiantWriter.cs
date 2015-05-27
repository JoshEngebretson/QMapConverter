using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toe;

namespace Map.Writers
{
    /// <summary>
    /// Writes a .map file using GtkRadiant's brush primitive format
    /// 
    /// Tricky part will be calculating the homogenous matrix?
    /// </summary>
    [QMapConverter.WriterMode("gtk")]
    public class GtkRadiantWriter : AbstractMapWriter
    {
        public override bool CheckArgs(string[] rawArgs, Dictionary<string, string> arguments)
        {
            return false;
        }

        public override void PrintUsage()
        {
            Console.WriteLine("Info Writer");
            Console.WriteLine("    QMapConverter gtk MapName.map OutputMapName.map");
        }

        public override void WriteMap(BrushMap map)
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(" GtkRadiant Writer");
            Console.WriteLine("----------------------------------------------------------------");
            throw new NotImplementedException();
        }

        Matrix3 ComputeTextureMatrix(Vector3 uVec, Vector3 vVec)
        {
            Vector3 topRow = uVec.Normalized();
            Vector3 botRow = Vector3.Cross(uVec, vVec).Normalized();
            Vector3 midRow = Vector3.Cross(botRow, uVec).Normalized();

            return new Matrix3(topRow, midRow, botRow);
        }
    }
}
