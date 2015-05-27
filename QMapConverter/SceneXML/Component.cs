using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QMapConverter.SceneXML
{
    public class Component : Node
    {
        public Component(XmlElement element, XmlDocument doc) : base(element, doc)
        {
        }
    }
}
