using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class TestBuild {

    [MenuItem("Test/TestBuildIos")]
    public static void Test()
    {
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        AssetBundleBuild build = new AssetBundleBuild();
        string[] assets = new string[1];
        string[] names = new string[1];
        assets[0] = "Assets/ArtResources/Prefabs/MyPrefab.prefab";
        names[0] = "ArtResources/Prefabs/MyPrefab.prefab";

        build.assetBundleName = "aaa";
        build.assetNames = assets;
        build.addressableNames = names;

        builds.Add(build);

        build = new AssetBundleBuild();
        assets = new string[1];
        assets[0] = "Assets/ArtResources/Materials/MyMaterial.mat";
        build.assetBundleName = "bbbbbb";
        build.assetNames = assets;
        builds.Add(build);

        build = new AssetBundleBuild();
        assets = new string[1];
        assets[0] = "Assets/ArtResources/Textures/bnt_Blue_S.png";
        build.assetBundleName = "ccccc";
        build.assetNames = assets;
        builds.Add(build);


        string outDir = Path.Combine(Application.dataPath, "../AssetBundles/Test");

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outDir, builds.ToArray(), BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);

        Debug.Log("####All Bundles " + manifest.GetAllAssetBundles().Length);
        foreach (string v in manifest.GetAllAssetBundles())
        {
            Debug.Log(v);
            Debug.Log("########Dependencies " + manifest.GetAllDependencies(v).Length);
            foreach (string k in manifest.GetAllDependencies(v))
            {
                Debug.Log(k);
            }

            Debug.Log("########Direct Dependencies " + manifest.GetDirectDependencies(v).Length);
            foreach (string k in manifest.GetDirectDependencies(v))
            {
                Debug.Log(k);
            }
        }

    }


    [MenuItem("Test/TestDep")]
    public static void TestDeps() {
        string p = "Assets/ArtResources/Prefabs/TestPrefab.prefab";
        var deps= AssetDatabase.GetDependencies(p,false);
        foreach(string dep in deps)
        {
            Debug.Log(dep);
        }
    }

    [MenuItem("Test/TestOhters")]
    public static void TestOthers()
    {
        var dataPath = System.IO.Path.GetFullPath(".");
        AssetBundle ab=AssetBundle.LoadFromFile(dataPath + "/AssetBundles/Test/aaa");
        Debug.Log(ab);
        foreach(string n in ab.GetAllAssetNames())
        {
            Debug.Log(n);
        }

        Object o = ab.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
        Debug.Log(o);

        ab.Unload(false);
    }
}
