using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FreeImageAPI.IO;
using FreeImageAPI;

namespace QMapConverter.SceneXML
{
    public class Material
    {
        static readonly string[] EXTENSIONLIST = { ".tga", ".png", ".jpg", ".jpeg", ".bmp", ".dds" };

        public string FullPath { get; private set; }
        public string ShortName { get; private set; }
        public string DiffuseTexName = "";
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Material(string mat, string parentPath)
        {
            FullPath = mat.Replace("\\","/");
            ShortName = mat.Replace(parentPath + "\\", "").Replace("\\", "/");
            XmlDocument doc = new XmlDocument();
            doc.Load(mat);


            foreach (XmlElement texElem in doc.DocumentElement.GetElementsByTagName("texture"))
            {
                if (texElem.GetAttribute("unit").Equals("diffuse"))
                {
                    DiffuseTexName = texElem.GetAttribute("name");
                    foreach (string ext in EXTENSIONLIST)
                    {
                        string path = System.IO.Path.Combine(parentPath, DiffuseTexName) + ext;
                        if (System.IO.File.Exists(path))
                        {
                            FIBITMAP image = FreeImage.LoadEx(path);
                            Width = (int)FreeImage.GetWidth(image);
                            Height = (int)FreeImage.GetHeight(image);
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }
}
