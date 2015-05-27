using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMapConverter.SceneXML
{
    public class MaterialDatabase : List<Material>
    {
        public MaterialDatabase(string path)
        {
            seekMaterials(path);
        }

        void seekMaterials(string path)
        {
            foreach (string d in System.IO.Directory.EnumerateDirectories(path))
            {
                System.IO.DirectoryInfo dinfo = new System.IO.DirectoryInfo(d);
                if (dinfo.Name.ToLower().Equals("materials"))
                    fill(d, path);
                else
                    seekMaterials(d);
            }
        }

        void fill(string path, string srcPath)
        {
            foreach (string f in System.IO.Directory.EnumerateFiles(path))
                Add(new Material(f, srcPath));
            foreach (string d in System.IO.Directory.EnumerateDirectories(path))
                fill(d, srcPath);
        }

        public string GetMaterial(string texName)
        {
            foreach (Material m in this)
            {
                if (m.ShortName.ToLower().Equals(texName.ToLower()))
                    return m.ShortName;
            }
            foreach (Material m in this)
                if (m.DiffuseTexName != null && m.DiffuseTexName.ToLower().Equals(texName.ToLower()))
                    return m.ShortName;
            return null;
        }
    }
}
