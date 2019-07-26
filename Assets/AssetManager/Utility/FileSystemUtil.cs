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

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
