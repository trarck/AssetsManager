using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using YH.AssetManager;

namespace UnityEngine.AssetBundles.AssetBundleDataSource
{
    public class AssetDatabaseABDataSource : ABDataSource
    {
        public static List<ABDataSource> CreateDataSources()
        {
            var op = new AssetDatabaseABDataSource();
            var retList = new List<ABDataSource>();
            retList.Add(op);
            return retList;
        }

        public string Name {
            get {
                return "Default";
            }
        }

        public string ProviderName {
            get {
                return "Built-in";
            }
        }

        public string[] GetAssetPathsFromAssetBundle (string assetBundleName) {
            return AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
        }

        public string GetAssetBundleName(string assetPath) {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null) {
                return string.Empty;
            }
            var bundleName = importer.assetBundleName;
            if (importer.assetBundleVariant.Length > 0) {
                bundleName = bundleName + "." + importer.assetBundleVariant;
            }
            return bundleName;
        }

        public string GetImplicitAssetBundleName(string assetPath) {
            return AssetDatabase.GetImplicitAssetBundleName (assetPath);
        }

        public string[] GetAllAssetBundleNames() {
            return AssetDatabase.GetAllAssetBundleNames ();
        }

        public bool IsReadOnly() {
            return false;
        }

        public void SetAssetBundleNameAndVariant (string assetPath, string bundleName, string variantName) {
            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, variantName);
        }

        public void RemoveUnusedAssetBundleNames() {
            AssetDatabase.RemoveUnusedAssetBundleNames ();
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

          //  ClearTempManifest(info.outputDirectory);

            SaveBundleManifest(buildManifest,info.outputDirectory);

            foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
            {
                if (info.onBuild != null)
                {
                    info.onBuild(assetBundleName);
                }
            }
            return true;
        }

        void ClearTempManifest(string outDir)
        {
            YH.FileSystemUtil.RemoveDirectoryFiles(outDir, @".*\.manifest");
        }

        void SaveBundleManifest(AssetBundleManifest buildManifest,string outDir)
        {
            BundleManifest bundleManifest = new BundleManifest();

            List<YH.AssetManager.AssetBundleInfo> all = new List<YH.AssetManager.AssetBundleInfo>();

            if (AssetBundleModel.Model.BundleListIsEmpty())
            {
                AssetBundleModel.Model.Rebuild();
            }

            foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
            {
                AssetBundleModel.BundleDataInfo bundleInfo = AssetBundleModel.Model.FindBundle(new AssetBundleModel.BundleNameData(assetBundleName)) as AssetBundleModel.BundleDataInfo;
                Debug.Log(AssetBundleModel.Model.FindBundle(new AssetBundleModel.BundleNameData(assetBundleName)));
                if (bundleInfo != null)
                {
                    YH.AssetManager.AssetBundleInfo assetBundleInfo = new YH.AssetManager.AssetBundleInfo();
                    assetBundleInfo.fullName = bundleInfo.m_Name.fullNativeName;
                    assetBundleInfo.shortName = bundleInfo.m_Name.shortName;
                    assetBundleInfo.size = (int)bundleInfo.size;
                    assetBundleInfo.hash = buildManifest.GetAssetBundleHash(assetBundleName).ToString();
                    assetBundleInfo.dependencies = buildManifest.GetDirectDependencies(assetBundleName);

                    List<string> assets = new List<string>();
                    foreach(AssetBundleModel.AssetInfo assetInfo in bundleInfo.GetConcretes())
                    {
                        Debug.Log(assetInfo.displayName + "," + assetInfo.bundleName + "," + assetInfo.fullAssetName);
                        assets.Add(assetInfo.fullAssetName);
                    }
                    assetBundleInfo.assets = assets.ToArray();

                    all.Add(assetBundleInfo);
                }
            }
            bundleManifest.bundleInfos = all;

            string content = JsonUtility.ToJson(bundleManifest);

            System.IO.File.WriteAllText(Path.Combine(outDir, "all.manifest"), content);
        }
        
    }
}
