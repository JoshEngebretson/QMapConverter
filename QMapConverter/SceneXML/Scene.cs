using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QMapConverter.SceneXML
{
    public class Scene : Node
    {
        public Scene(string rootName)
        {
            backingDoc = new XmlDocument();
            element = backingDoc.CreateElement(rootName);
            element.SetAttribute("id", GetNextID().ToString());
            backingDoc.AppendChild(element);
        }

        public void Save(string file)
        {
            backingDoc.Save(file);
        }
    }
}
