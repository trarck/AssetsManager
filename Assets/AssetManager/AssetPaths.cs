using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace YH.AssetManager
{
    public class AssetPaths
    {
#if UNITY_EDITOR
        public static string assetsPath = Application.dataPath;
#else
        public static string assetsPath=Application.streamingAssetsPath;
#endif
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
    }
}
