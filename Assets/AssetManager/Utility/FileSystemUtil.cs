using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace YH
{
    public class FileSystemUtil
    {
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
		
		public static void RemoveDirectoryFiles(string path,string pattern)
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
                    Debug.Log(f.Name + ":" + reg.IsMatch(f.Name));

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
    }
}
