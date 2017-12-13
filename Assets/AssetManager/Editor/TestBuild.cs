using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class TestBuild {

    [MenuItem("Test/TestBuildIos")]
    public static void Test ()
    {
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        AssetBundleBuild build = new AssetBundleBuild();
        string[] assets = new string[1];
        assets[0] = "Assets/ArtResources/Prefabs/TestPrefab.prefab";

        build.assetBundleName = "mytest";
        build.assetNames = assets;

        builds.Add(build);

        build = new AssetBundleBuild();
        assets = new string[1];
        assets[0] = "Assets/ArtResources/Materials/MyMaterial.prefab";
        build.assetBundleName = "mymat";
        build.assetNames = assets;
        builds.Add(build);

        build = new AssetBundleBuild();
        assets = new string[1];
        assets[0] = "Assets/ArtResources/Textures/bnt_Blue_S.png";
        build.assetBundleName = "mypng";
        build.assetNames = assets;
        builds.Add(build);


        string outDir = Path.Combine(Application.dataPath, "../AssetBundles/Test");

        AssetBundleManifest manifest= BuildPipeline.BuildAssetBundles(outDir, builds.ToArray(), BuildAssetBundleOptions.None,EditorUserBuildSettings.activeBuildTarget);

        Debug.Log("####All Bundles "+ manifest.GetAllAssetBundles().Length);
        foreach(string v in manifest.GetAllAssetBundles())
        {
            Debug.Log(v);
            Debug.Log("########Dependencies "+ manifest.GetAllDependencies(v).Length);
            foreach (string k in manifest.GetAllDependencies(v))
            {
                Debug.Log(k);
            }

            Debug.Log("########Direct Dependencies "+ manifest.GetDirectDependencies(v).Length);
            foreach (string k in manifest.GetDirectDependencies(v))
            {
                Debug.Log(k);
            }
        }
        
    }
}
