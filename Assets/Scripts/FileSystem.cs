using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace YH
{
    public class FileSystem
    {

        public static string dataPath = Application.dataPath;

        public static string streamingAssetsPath = Application.streamingAssetsPath;

        public static string bundlesPath="AssetBundles";

        public static string bundleManifestFile = "all.manifest";

        public static string bundleOutPaths = "AssetBundles";

        public static string assetPathPrev = "Assets/";

        public static List<string> searchPaths = new List<string>();

        /// <summary>
        /// 获取文件的全路径
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FullPathForFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return filename;
            }

            if (Path.IsPathRooted(filename))
            {
                return filename;
            }

            //get from search path
            var fullPath = GetFullPathFromSearchPaths(filename);
            if (!string.IsNullOrEmpty(fullPath))
            {
                return fullPath;
            }

            //streaming path
            return StreamingPathForFilename(filename);
        }

        /// <summary>
        /// 在搜索目录查找文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 添加一个搜索路径
        /// </summary>
        /// <param name="path"></param>
        public static void AddSearchPath(string path)
        {
            if (!searchPaths.Contains(path))
            {
                searchPaths.Add(path);
            }
        }

        /// <summary>
        /// 插入一个搜索路径
        /// </summary>
        /// <param name="index"></param>
        /// <param name="path"></param>
        public static void InsertSearchPath(int index,string path)
        {
            if (!searchPaths.Contains(path))
            {
                searchPaths.Insert(index, path);
            }
        }

        /// <summary>
        /// 插入一组搜索路径
        /// </summary>
        /// <param name="index"></param>
        /// <param name="paths"></param>
        public static void InsertRangeSearchPath(int index, string[] paths)
        {
            searchPaths.InsertRange(index, paths);
        }

        /// <summary>
        /// 移除搜索路径
        /// </summary>
        /// <param name="path"></param>
        public static void RemoveSearchPath(string path)
        {
            if (!searchPaths.Contains(path))
            {
                searchPaths.Remove(path);
            }
        }

        /// <summary>
        /// 直接获取streaming的url
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string StreamingUrlForFilename(string filename)
        {
#if UNITY_ANROID
            return "jar:file://" + Application.dataPath + "!/assets/" + filename;
#else
            return "file://" + Combine(Application.streamingAssetsPath, filename);
#endif
        }

        /// <summary>
        /// 获取Streaming下文件路径
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string StreamingPathForFilename(string filename)
        {
            return Combine(Application.streamingAssetsPath, filename);
        }

        /// <summary>
        /// 合并路径
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 归一化路径
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string NormalizeFilename(string filename)
        {
            return filename.Replace("\\", "/");
        }

        /// <summary>
        /// 获取路径的相对位置
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        /// <returns></returns>
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
            for (int l=froms.Length>tos.Length?tos.Length: froms.Length; i < l; ++i)
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

        /// <summary>
        /// 移除路径中的Assets
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RemoveAssetPrev(string path)
        {
            if (!string.IsNullOrEmpty(path) && path.StartsWith(assetPathPrev,System.StringComparison.CurrentCultureIgnoreCase))
            {
                return path.Substring(assetPathPrev.Length);
            }
            return path;
        }

        /// <summary>
        /// 添加Assets到路径。如果存在则不添加。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AddAssetPrev(string path)
        {
            if (!string.IsNullOrEmpty(path) && !path.StartsWith(assetPathPrev, System.StringComparison.CurrentCultureIgnoreCase))
            {
                return Combine(assetPathPrev, path);
            }
            return path;
        }

        /// <summary>
        /// 强制删除一个目录。包括自己
        /// </summary>
        /// <param name="path"></param>
        public static void ForceDeleteDirectory(string path)
        {
            DirectoryInfo root;
            Stack<DirectoryInfo> fols;
            DirectoryInfo fol;
            fols = new Stack<DirectoryInfo>();
            root = new DirectoryInfo(path);
            fols.Push(root);
            while (fols.Count > 0)
            {
                fol = fols.Pop();
                fol.Attributes = fol.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                foreach (DirectoryInfo d in fol.GetDirectories())
                {
                    fols.Push(d);
                }
                foreach (FileInfo f in fol.GetFiles())
                {
                    f.Attributes = f.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                    f.Delete();
                }
            }
            root.Delete(true);
        }

        /// <summary>
        /// 清空一个目录。
        /// </summary>
        /// <param name="path"></param>
        public static void ForceClearDirectory(string path)
        {
            DirectoryInfo root;
            Stack<DirectoryInfo> fols;
            DirectoryInfo fol;
            fols = new Stack<DirectoryInfo>();
            root = new DirectoryInfo(path);
            fols.Push(root);
            while (fols.Count > 0)
            {
                fol = fols.Peek();
                fol.Attributes = fol.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);

                foreach (FileInfo f in fol.GetFiles())
                {
                    f.Attributes = f.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                    f.Delete();
                }

                DirectoryInfo[] subs = fol.GetDirectories();
                if (subs.Length > 0)
                {
                    foreach (DirectoryInfo d in fol.GetDirectories())
                    {
                        fols.Push(d);
                    }
                }
                else
                {
                    if (fol != root)
                    {
                        fol.Delete(true);
                    }

                    fols.Pop();
                }
            }
        }

        /// <summary>
        /// 按照规则移除一个目录下的文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        public static void RemoveDirectoryFiles(string path, string pattern)
        {
            DirectoryInfo root;
            Stack<DirectoryInfo> fols;
            DirectoryInfo fol;
            fols = new Stack<DirectoryInfo>();
            root = new DirectoryInfo(path);
            fols.Push(root);
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            while (fols.Count > 0)
            {
                fol = fols.Peek();
                foreach (FileInfo f in fol.GetFiles())
                {
                    if (reg.IsMatch(f.Name))
                    {
                        f.Attributes = f.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                        f.Delete();
                    }
                }

                DirectoryInfo[] subs = fol.GetDirectories();
                if (subs.Length > 0)
                {
                    foreach (DirectoryInfo d in fol.GetDirectories())
                    {
                        fols.Push(d);
                    }
                }
                else
                {
                    fols.Pop();
                }
            }
        }
        
        public List<string> SearchFiles(string path, string pattern = null)
        {
            DirectoryInfo startInfo = new DirectoryInfo(path);
            if (!startInfo.Exists)
            {
                return null;
            }

            List<string> result = new List<string>();

            Stack<DirectoryInfo> dirs = new Stack<DirectoryInfo>();
            dirs.Push(startInfo);

            DirectoryInfo dir;

            bool haveFilter = false;
            System.Text.RegularExpressions.Regex reg = null;
            if (!string.IsNullOrEmpty(pattern))
            {
                haveFilter = true;
                reg = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            while (dirs.Count > 0)
            {
                dir = dirs.Pop();

                foreach (FileInfo fi in dir.GetFiles())
                {
                    if (!haveFilter || reg.IsMatch(fi.FullName))
                    {
                        result.Add(fi.FullName);
                    }
                }

                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    if (!subDir.Name.StartsWith("."))
                    {
                        dirs.Push(subDir);
                    }
                }
            }
            return result;
        }
    }
}
