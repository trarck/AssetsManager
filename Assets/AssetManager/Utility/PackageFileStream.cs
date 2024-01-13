using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YH.AssetManage
{
    public interface IPackageFileStream
    {
        Stream Open(string path);

        bool Exists(string path);
    }

    public class PackageFileStream
    {
       private static IPackageFileStream _Impl = null;

       public static IPackageFileStream Impl
        {
            get
            {
                if (_Impl == null)
                {
#if UNITY_ANDROID
                    _Impl = new PackageFileStreamAndroid();
#else
                    _Impl = new FileStreamHelperCommon();
#endif
                }

                return _Impl;
            }
        }
    }

    public class PackageFileStreamCommon: IPackageFileStream
    {
        public Stream Open(string path)
        {
            return new FileStream(path, FileMode.Open);
        }

        public bool Exists(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }
            return Directory.Exists(path);
        }
    }

    public class PackageFileStreamAndroid:IPackageFileStream
    {
        private const string ACTIVITY_JAVA_CLASS = "com.unity3d.player.UnityPlayer";
        private const string ASSET_MANGER_CLASS_NAME = "android.content.res.AssetManager";
        private static readonly string SplitFlag = "!/assets/";

        private static AndroidJavaObject _AssetManager;

        private static Dictionary<string, HashSet<string>> _FolderLookup =
            new Dictionary<string, HashSet<string>>();

        protected static AndroidJavaObject AssetManager
        {
            get
            {
                if (_AssetManager != null)
                    return _AssetManager;

                try
                {
                    using (AndroidJavaClass activityClass = new AndroidJavaClass(ACTIVITY_JAVA_CLASS))
                    {
                        using (var context = activityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                        {
                            _AssetManager = context.Call<AndroidJavaObject>("getAssets");
                        }
                    }
                }
                catch (Exception e)
                {
                    AMDebug.LogWarningFormat("Failed to get the AssetManager from the Activity, try to get it from android.content.res.AssetManager.ex:{0}", e);
                }

                try
                {
                    if (_AssetManager == null)
                    {
                        using (AndroidJavaClass assetManagerClass = new AndroidJavaClass(ASSET_MANGER_CLASS_NAME))
                        {
                            _AssetManager = assetManagerClass.GetStatic<AndroidJavaObject>("getSystem");
                        }
                    }
                }
                catch (Exception e)
                {
                    AMDebug.LogErrorFormat("Failed to get the AssetManager from android.content.res.AssetManager ex:{0}", e);
                }
                return _AssetManager;
            }
        }

        protected static string GetAssetFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            int start = path.LastIndexOf(SplitFlag);
            if (start < 0)
                return path;

            return path.Substring(start + SplitFlag.Length);
        }

        public bool Exists(string path)
        {
            try
            {
                using (AndroidJavaObject fileDescriptor = AssetManager.Call<AndroidJavaObject>("openFd", GetAssetFilePath(path)))
                {
                    if (fileDescriptor != null)
                        return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        public bool Existed(string filePath)
        {
            var fileDir = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            var fileTable = GetFilesInDir(fileDir);
            return fileTable.Contains(fileName);
        }

        private static HashSet<string> GetFilesInDir(string dir)
        {
            dir = GetAssetFilePath(dir);

            HashSet<string> fileTable;
            if (!_FolderLookup.TryGetValue(dir, out fileTable))
            {
                fileTable = new HashSet<string>();
                var files = AssetManager.Call<string[]>("list", dir);
                foreach (var file in files)
                {
                    fileTable.Add(file);
                }

                _FolderLookup.Add(dir, fileTable);
            }

            return fileTable;
        }

        public Stream Open(string path)
        {
            return new AndroidAssetStream(AssetManager.Call<AndroidJavaObject>("open", GetAssetFilePath(path)));
        }
    }
}
