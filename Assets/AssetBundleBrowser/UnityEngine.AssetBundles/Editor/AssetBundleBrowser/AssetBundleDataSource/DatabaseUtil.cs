using System.IO;
using System.Collections.Generic;

using YH.AssetManager;

namespace UnityEngine.AssetBundles.AssetBundleDataSource
{
    public class DatabaseUtil
    {
         public static void ClearTempManifest(string outDir)
        {
            YH.FileSystemUtil.RemoveDirectoryFiles(outDir, @".*\.manifest");
        }

        public static void SaveBundleManifest(AssetBundleManifest buildManifest, string outDir)
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

                    List<AssetInfo> assets = new List<AssetInfo>();
                    foreach (AssetBundleModel.AssetInfo assetInfo in bundleInfo.GetConcretes())
                    {
                        AssetInfo ai = new AssetInfo();
                        Debug.Log(assetInfo.displayName + "," + assetInfo.bundleName + "," + assetInfo.fullAssetName);
                        ai.fullName = assetInfo.fullAssetName;

                        assets.Add(ai);
                        //assets.Add(AssetPaths.RemoveAssetPrev(assetInfo.fullAssetName));
                    }
                    assetBundleInfo.assets = assets;

                    all.Add(assetBundleInfo);
                }
            }
            bundleManifest.bundleInfos = all;

            string content = JsonUtility.ToJson(bundleManifest);

            System.IO.File.WriteAllText(Path.Combine(outDir, "all.manifest"), content);
        }
    }
}