using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace UnityEngine.AssetBundles.AssetBundleDataSource
{
    [Serializable]
    public class AssetDatabaseItem
    {
        public string path;
        public string bundleName;
        public string variantName;
    }

    public class JsonAssetDatabase
    {
        public List<AssetDatabaseItem> assets;
    }

    public class AssetDatabaseJsonDataSource : ABDataSource
    {
        protected Dictionary<string, AssetDatabaseItem> m_Assets;
        protected Dictionary<string, HashSet<string>> m_Bundles;

        public static List<ABDataSource> CreateDataSources()
        {
            var op = new AssetDatabaseJsonDataSource();
            op.Init();
            op.Load();

            var retList = new List<ABDataSource>();
            retList.Add(op);
            return retList;
        }

        public string Name {
            get {
                return "JsonDataSource";
            }
        }

        public string ProviderName {
            get {
                return "JsonDataSource";
            }
        }

        public string[] GetAssetPathsFromAssetBundle (string assetBundleName)
        {
            if (m_Bundles.ContainsKey(assetBundleName))
            {
                return m_Bundles[assetBundleName].ToArray();
            }
            return new string[0];
        }

        public string GetAssetBundleName(string assetPath) {
            if (!m_Assets.ContainsKey(assetPath)) {
                return string.Empty;
            }
            AssetDatabaseItem item = m_Assets[assetPath];

            var bundleName = item.bundleName;
            if (item.variantName.Length > 0) {
                bundleName = bundleName + "." + item.variantName;
            }
            return bundleName;
        }

        public string GetImplicitAssetBundleName(string assetPath) {
            if(m_Assets.ContainsKey(assetPath))
            {
                return m_Assets[assetPath].bundleName;
            }
            return null;
        }

        public string[] GetAllAssetBundleNames() {
            return m_Bundles.Keys.ToArray();
        }

        public bool IsReadOnly() {
            return false;
        }

        public void SetAssetBundleNameAndVariant (string assetPath, string bundleName, string variantName) {
            if (m_Assets.ContainsKey(assetPath))
            {
                AssetDatabaseItem item = m_Assets[assetPath];

                var oldBundleName = item.bundleName;

                //无论如何都要把上次的asset从bundle中移除。
                m_Bundles[oldBundleName].Remove(assetPath);

                if (string.IsNullOrEmpty(bundleName))
                {
                    //asset的bundle name为空，则表示删除asset.
                    m_Assets.Remove(assetPath);
                }
                else
                {
                    if (oldBundleName != bundleName)
                    {
                        item.bundleName = bundleName;

                        if (!m_Bundles.ContainsKey(bundleName))
                        {
                            m_Bundles[bundleName] = new HashSet<string>();
                        }

                        m_Bundles[bundleName].Add(assetPath);
                    }

                    item.variantName = variantName;
                }
            }
            else
            {
                AssetDatabaseItem item = new AssetDatabaseItem();
                item.path = assetPath;
                item.bundleName = bundleName;
                item.variantName = variantName;

                m_Assets[assetPath] = item;

                if (!string.IsNullOrEmpty(bundleName))
                {

                    if (!m_Bundles.ContainsKey(bundleName))
                    {
                        m_Bundles[bundleName] = new HashSet<string>();
                    }

                    m_Bundles[bundleName].Add(assetPath);
                }
            }

            Save();
        }

        public void RemoveUnusedAssetBundleNames()
        {
            List<string> bundleNames = m_Bundles.Keys.ToList();
            foreach(string bundleName in bundleNames)
            {
                if (m_Bundles[bundleName].Count == 0)
                {
                    m_Bundles.Remove(bundleName);
                }
            }
        }

        public bool CanSpecifyBuildTarget { 
            get { return true; } 
        }
        public bool CanSpecifyBuildOutputDirectory { 
            get { return true; } 
        }

        public bool CanSpecifyBuildOptions { 
            get { return true; } 
        }

        public bool BuildAssetBundles (ABBuildInfo info) {
            var buildManifest = BuildPipeline.BuildAssetBundles(info.outputDirectory, info.options, info.buildTarget);
            if (buildManifest == null)
                return false;
            foreach(var assetBundleName in buildManifest.GetAllAssetBundles())
            {
                if (info.onBuild != null)
                {
                    info.onBuild(assetBundleName);
                }
            }
            return true;
        }

        public void Init()
        {
            m_Assets = new Dictionary<string, AssetDatabaseItem>();
            m_Bundles = new Dictionary<string, HashSet<string>>();
        }

        void BuildData(JsonAssetDatabase db)
        {
            if (db != null)
            {
                foreach (AssetDatabaseItem item in db.assets)
                {
                    m_Assets[item.path] = item;
                    if (!m_Bundles.ContainsKey(item.bundleName))
                    {
                        m_Bundles[item.bundleName] = new HashSet<string>();
                    }
                    m_Bundles[item.bundleName].Add(item.path);
                }
            }
        }

        public void Load()
        {
            var dataPath = System.IO.Path.GetFullPath(".");
            var dataFile = Path.Combine(dataPath, AssetBundleConstans.JsonDatabaseFile);
            if (File.Exists(dataFile))
            {
                string cnt = File.ReadAllText(dataFile);
                JsonAssetDatabase db = JsonUtility.FromJson<JsonAssetDatabase>(cnt);
                BuildData(db);
            }
        }

        public void Save()
        {
            JsonAssetDatabase db = new JsonAssetDatabase();
            db.assets = m_Assets.Values.ToList();
            string cnt = JsonUtility.ToJson(db);

            var dataPath = Path.GetFullPath(".");
            var dataFile = Path.Combine(dataPath, AssetBundleConstans.JsonDatabaseFile);

            Debug.Log(Path.GetDirectoryName(dataFile));

            if (!Directory.Exists(Path.GetDirectoryName(dataFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dataFile));
            }

            File.WriteAllText(dataFile, cnt);
        }
    }
}
