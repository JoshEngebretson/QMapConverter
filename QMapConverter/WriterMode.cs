using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMapConverter
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WriterMode : Attribute
    {
        string m;
        public WriterMode(string md)
        {
            m = md;
        }

        public string mode() { return m; }
    }

    public static class WriterSelect
    {
        public static Map.AbstractMapWriter SelectWriter(string name)
        {
            foreach (Type t in typeof(WriterSelect).Assembly.GetTypes())
            {
                WriterMode mode = Attribute.GetCustomAttribute(t, typeof(WriterMode)) as WriterMode;
                if (mode != null)
                {
                    if (mode.mode().Equals(name))
                        return Activator.CreateInstance(t) as Map.AbstractMapWriter;
                }
            }
            return null;
        }
    }
}
