using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public override void WriteMap(string outputFile, BrushMap map)
        {
            throw new NotImplementedException();
        }
    }
}
