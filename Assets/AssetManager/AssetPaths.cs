using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace YH.AssetManager
{
    public class AssetPaths
    {

        public static string dataPath = Application.dataPath;

        public static string streamingAssetsPath = Application.streamingAssetsPath;

        public static string bundlesPath="AssetBundles";

        public static List<string> searchPaths = new List<string>();

        public static string FullPathForFilename(string filename)
        {
            if (Path.IsPathRooted(filename))
            {
                return filename;
            }

            foreach(string searchPath in searchPaths)
            {
                string fullPath = Path.Combine(searchPath, filename);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }

        public static void AddSearchPath(string path)
        {
            if (!searchPaths.Contains(path))
            {
                searchPaths.Add(path);
            }
        }

        public static void RemoveSearchPath(string path)
        {
            if (!searchPaths.Contains(path))
            {
                searchPaths.Remove(path);
            }
        }

        public static string StreamingPathForFilename(string filePath)
        {
            
#if UNITY_ANROID
            return "jar:file://" + Application.dataPath + "!/assets/" + filePath;
#elif unity_IOS
            return "file://" + Application.dataPath + "/Raw/" + filePath;
#else
            return "file://" + Application.dataPath + "/StreamingAssets/" + filePath;
#endif
        }

        public static string Combine(params string[] paths)
        {
            if (paths.Length == 0) return string.Empty;
            if (paths.Length == 1) return paths[0];

            StringBuilder sb = new StringBuilder();

            string c=null,n=null;            

            int start = 0;
            for (; start < paths.Length; ++start)
            {
                c = paths[start];
                if (!string.IsNullOrEmpty(c))
                {
                    sb.Append(c);
                    break;
                }
            }            

            for(int i = start+1; i < paths.Length; ++i)
            {
                n = paths[i];
                if (string.IsNullOrEmpty(n))
                {
                    continue;
                }
    
                if((c.EndsWith("/") || c.EndsWith("\\")))
                {
                    if(n.StartsWith("/") || n.StartsWith("\\"))
                    {
                        sb.Append(n.Substring(1));
                    }
                    else
                    {
                        sb.Append(n);
                    }
                }
                else
                {
                    if (n.StartsWith("/") || n.StartsWith("\\"))
                    {
                        sb.Append(n);
                    }
                    else
                    {
                        sb.Append(Path.DirectorySeparatorChar);
                        sb.Append(n);
                    }
                }
                c = n;
            }
            return sb.ToString();
        }

        public static string NormalizeFilename(string filename)
        {
            return filename.Replace("\\", "/");
        }
    }
}
