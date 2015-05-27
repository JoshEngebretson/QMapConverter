using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Toe;

namespace QMapConverter.SceneXML
{
    public class Node
    {
        static int nextID = 1;

        protected static int GetNextID()
        {
            int ret = nextID;
            ++nextID;
            return ret;
        }

        protected XmlElement element;
        protected XmlDocument backingDoc;

        protected Node()
        {

        }

        protected Node(XmlElement element, XmlDocument doc)
        {
            this.element = element;
            backingDoc = doc;
        }

        public Node CreateChild()
        {
            return CreateChild(null);
        }

        public Node CreateChild(string name)
        {
            XmlElement elem = backingDoc.CreateElement("node");
            element.AppendChild(elem);
            Node ret = new Node(elem, backingDoc);
            elem.SetAttribute("id", GetNextID().ToString());
            if (name != null && name.Length > 0)
                elem.SetAttribute("name", name);
            return ret;
        }

        public Component CreateComponent(string type)
        {
            XmlElement elem = backingDoc.CreateElement("component");
            element.AppendChild(elem);
            Component ret = new Component(elem, backingDoc);
            elem.SetAttribute("type", type);
            elem.SetAttribute("id", GetNextID().ToString());
            return ret;
        }

        public void SetAttribute(string name, string value)
        {
            XmlElement attrElem = backingDoc.CreateElement("attribute");
            attrElem.SetAttribute("name", name);
            attrElem.SetAttribute("value", value);
            element.AppendChild(attrElem);
        }

        public void SetAttribute(string name, int value)
        {
            XmlElement attrElem = backingDoc.CreateElement("attribute");
            attrElem.SetAttribute("name", name);
            attrElem.SetAttribute("value", value.ToString());
            element.AppendChild(attrElem);
        }

        public void SetAttribute(string name, float value)
        {
            XmlElement attrElem = backingDoc.CreateElement("attribute");
            attrElem.SetAttribute("name", name);
            attrElem.SetAttribute("value", value.ToString());
            element.AppendChild(attrElem);
        }

        public void SetAttribute(string name, bool value)
        {
            XmlElement attrElem = backingDoc.CreateElement("attribute");
            attrElem.SetAttribute("name", name);
            attrElem.SetAttribute("value", value ? "true" : "false");
            element.AppendChild(attrElem);
        }

        public void SetAttribute(string name, Vector3 value)
        {
            XmlElement attrElem = backingDoc.CreateElement("attribute");
            attrElem.SetAttribute("name", name);
            attrElem.SetAttribute("value", value.X + " " + value.Y + " " + value.Z);
            element.AppendChild(attrElem);
        }

        public void SetAttribute(string name, Vector2 value)
        {
            XmlElement attrElem = backingDoc.CreateElement("attribute");
            attrElem.SetAttribute("name", name);
            attrElem.SetAttribute("value", value.X + " " + value.Y);
            element.AppendChild(attrElem);
        }

        public void WriteVariables(Dictionary<string,string> properties)
        {
            XmlElement attr = backingDoc.CreateElement("attribute");
            element.AppendChild(attr);

            attr.SetAttribute("name", "Variables");
            foreach (string key in properties.Keys)
            {
                XmlElement variant = backingDoc.CreateElement("variant");
                attr.AppendChild(variant);
                variant.SetAttribute("hash", StringHash(key).ToString());

                string value = properties[key];
                int junkInt = 0;
                float junkFloat = 0;
                if (value.ToLower().Equals("true") || value.ToLower().Equals("false"))
                    variant.SetAttribute("type", "Bool");
                else if (int.TryParse(value, out junkInt))
                    variant.SetAttribute("type", "Float");
                else if (float.TryParse(value, out junkFloat))
                    variant.SetAttribute("type", "Int");
                else
                    variant.SetAttribute("type", "String");
                variant.SetAttribute("value", value);
            }

            WriteVariableNames(properties);
        }

        //<attribute name="Variable Names" value="Test;TestAgain" />
        void WriteVariableNames(Dictionary<string,string> properties)
        {
            XmlElement attr = backingDoc.CreateElement("attribute");
            element.AppendChild(attr);

            attr.SetAttribute("name", "Variable Names");
            string str = "";
            foreach (string key in properties.Keys)
            {
                if (str.Length > 0)
                    str += ";";
                str += key;
            }
            attr.SetAttribute("value", str);
        }

        static ulong StringHash(string str)
        {
            ulong hash = 0;

            if (str.Length == 0)
                return hash;

            for (int i = 0; i < str.Length; ++i)
            {
                // Perform the actual hashing as case-insensitive
                char c = str[i];
                hash = c + (hash << 6) + (hash << 16) - hash;
                //hash = SDBMHash(hash, Convert.ToByte(char.ToLower(c)));
            }

            return hash;
        }

        static uint SDBMHash(uint hash, byte c) { return c + (hash << 6) + (hash << 16) - hash; }
    }
}
