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

        public static string bundleManifestFile = "all.manifest";

        public static string bundleOutPaths = "AssetBundles";

        public static string assetPathPrev = "Assets/";

        //远程下载地址，指向要下载的asset bundle的根目录。如https://www.xxx.com/xxx/Android。
        public static string remoteUrl = "";

        public static List<string> searchPaths = new List<string>();

        public static string FullPathForFilename(string filename)
        {
            if (Path.IsPathRooted(filename))
            {
                return filename;
            }

            return GetFullPathFromSearchPaths(filename);
        }

        public static string GetFullPathFromSearchPaths(string filename)
        {
            foreach (string searchPath in searchPaths)
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

        public static void InsertSearchPath(int index,string path)
        {
            if (!searchPaths.Contains(path))
            {
                searchPaths.Insert(index, path);
            }
        }

        //public static void InsertRangeSearchPath(int index, string[] paths)
        //{
        //    searchPaths.InsertRange(index, paths);
        //}

        public static void RemoveSearchPath(string path)
        {
            if (!searchPaths.Contains(path))
            {
                searchPaths.Remove(path);
            }
        }

        public static string StreamingUrlForFilename(string filename)
        {
#if UNITY_ANROID
            return "jar:file://" + Application.dataPath + "!/assets/" + filename;
#else
            return "file://" + Combine(Application.streamingAssetsPath, filename);
#endif
        }

        public static string StreamingPathForFilename(string filename)
        {
            return Combine(Application.streamingAssetsPath, filename);
        }


        public static string GetFullPath(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return filename;
            }

            //filename is root and exists
            if (Path.IsPathRooted(filename))
            {
                if (File.Exists(filename))
                {
                    return filename;
                }
            }

            //get from search path
            var fullPath= GetFullPathFromSearchPaths(filename);
            if (!string.IsNullOrEmpty(fullPath))
            {
                return fullPath;
            }

            
#if ASSET_BUNDLE_REMOTE_UNITY
            //down load from remote
            return GetUrl(filename);
#else
            //get from package
            return StreamingPathForFilename(filename);  
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

        public static string RemoveAssetPrev(string path)
        {
            if (!string.IsNullOrEmpty(path) && path.StartsWith(assetPathPrev,System.StringComparison.CurrentCultureIgnoreCase))
            {
                return path.Substring(assetPathPrev.Length);
            }
            return path;
        }

        public static string AddAssetPrev(string path)
        {
            if (!string.IsNullOrEmpty(path) && !path.StartsWith(assetPathPrev, System.StringComparison.CurrentCultureIgnoreCase))
            {
                return Combine(assetPathPrev, path);
            }
            return path;
        }

        public static string GetBundlePath()
        {
            return Combine(Application.persistentDataPath, bundlesPath);
        }

        public static string ToBundlePath(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return filename;
            }

            //filename is root and exists
            if (Path.IsPathRooted(filename))
            {
                if (File.Exists(filename))
                {
                    return filename;
                }
            }
            //To bundle dir
            return Path.Combine(GetBundlePath(), filename);
        }

        public static string GetUrl(string filename)
        {
            if (filename.Contains("://"))
            {
                return filename;
            }

            if (remoteUrl.EndsWith("/"))
            {
                return remoteUrl +filename;
            }
            else
            {
                return remoteUrl + "/" + filename;
            }
        }
    }
}
