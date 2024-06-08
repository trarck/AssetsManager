using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;


namespace YH.AssetManage
{
    public class AssetPaths
    {
        private const string DirectorySeparatorChar = "/";
        public const string AssetBundelExt = ".ab";

        public static string dataPath = Application.dataPath;

        public static string streamingAssetsPath = Application.streamingAssetsPath;

        public static string bundlesPath="AssetBundles";

        public static string bundleManifestFile = "all.manifest";

        public static string bundlesEditorBuildOutPath = "AssetBundles";

        public static string assetPathPrev = "Assets/";

        //远程下载地址，指向要下载的asset bundle的根目录。如https://www.xxx.com/xxx/Android。
        public static string remoteUrl = "";

        public static List<string> searchPaths = new List<string>();

        private static string m_BundlesFullPath = null;
        private static string m_AndroidStreamingAssetsFullPath = null;

        private static ThreadLocal<StringBuilder> m_StringBuilderCache = new ThreadLocal<StringBuilder>();

        /// <summary>
        /// 设置默认路径
        /// 由于默认路径有重复检查，这里不做多次调用检查
        /// </summary>
        public static void SetupDefaultSearchPaths()
        {
            m_AndroidStreamingAssetsFullPath = dataPath + "!/assets/" + bundlesPath + "/";
            //add search paths
            AddSearchPath(GetBundlePath());
            AddSearchPath(Application.persistentDataPath);
#if UNITY_EDITOR && !ASSET_BUNDLE_REMOTE_ON
            //bunlde out path
            AddSearchPath(
                Combine(
                    Path.GetFullPath("."),
                    bundlesEditorBuildOutPath,
                    UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString()
                )
            );
#endif
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

        public static string StreamingUrlForFilename(string filename)
        {
#if UNITY_ANDROID
            return "jar:file://" + dataPath + "!/assets/" + filename;
#else
            return "file://" + Combine(streamingAssetsPath, filename);
#endif
        }

        public static string StreamingPathForFilename(string filename)
        {
            return Combine(streamingAssetsPath, bundlesPath, filename);
//#if UNITY_ANDROID
//            return m_AndroidStreamingAssetsFullPath + filename;
//#else
//            return Combine(streamingAssetsPath, bundlesPath, filename);
//#endif
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


#if ASSET_BUNDLE_REMOTE_ON
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

            if (!m_StringBuilderCache.IsValueCreated)
            {
                m_StringBuilderCache.Value = new StringBuilder();
            }
            StringBuilder sb = m_StringBuilderCache.Value;
            sb.Clear();

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
                        sb.Append(DirectorySeparatorChar);
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
            if (m_BundlesFullPath == null)
            {
                if (Path.IsPathRooted(bundlesPath))
                {
                    m_BundlesFullPath = bundlesPath;
                }
                else
                {
                    m_BundlesFullPath = Combine(Application.persistentDataPath, bundlesPath);
                }
            }
            return m_BundlesFullPath;
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
                return filename;
            }

            //To bundle dir
            return Combine(GetBundlePath(), filename);
        }

        public static string GetUrl(string filename)
        {
            if (filename.Contains("://"))
            {
                return filename;
            }

            if (string.IsNullOrEmpty(remoteUrl))
            {
                return StreamingUrlForFilename(filename);
            }
            else
            {
                if (remoteUrl.EndsWith("/"))
                {
                    return remoteUrl + filename;
                }
                else
                {
                    return remoteUrl + "/" + filename;
                }
            }
        }

		public static bool HaveRemoteUrl()
		{
			return !string.IsNullOrEmpty(remoteUrl);
		}

        public static string Relative(string fromPath, string toPath)
        {
            fromPath = fromPath.Replace("\\", "/");
            toPath = toPath.Replace("\\", "/");

            if (fromPath[fromPath.Length - 1] == '/')
            {
                fromPath = fromPath.Substring(0, fromPath.Length - 1);
            }

            if (toPath[toPath.Length - 1] == '/')
            {
                toPath = toPath.Substring(0, toPath.Length - 1);
            }

            string[] froms = fromPath.Split('/');
            string[] tos = toPath.Split('/');

            int i = 0;
            //look for same part
            for (int l = froms.Length > tos.Length ? tos.Length : froms.Length; i < l; ++i)
            {
                if (froms[i] != tos[i])
                {
                    break;
                }
            }

            if (i == 0)
            {
                //just windows. eg.fromPath=c:\a\b\c,toPath=d:\e\f\g
                //if linux the first is empty always same. eg. fromPath=/a/b/c,toPath=/d/e/f
                return toPath;
            }
            else
            {
                System.Text.StringBuilder result = new System.Text.StringBuilder();
                System.Text.StringBuilder toSB = new System.Text.StringBuilder();

                for (int j = i; j < froms.Length; ++j)
                {
                    result.Append("../");
                }

                for (int j = i; j < tos.Length; ++j)
                {
                    result.Append(tos[j]);
                    if (j < tos.Length - 1)
                    {
                        result.Append("/");
                    }
                }
                return result.ToString();
            }
        }
    }
}
