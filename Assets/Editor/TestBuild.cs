using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


public class TestBuild {

    [MenuItem("Test/Test")]
    public static void Test()
    {
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        AssetBundleBuild build = new AssetBundleBuild();
        string[] assets = new string[1];
        string[] names = new string[1];
        assets[0] = "Assets/ArtResources/Prefabs/MyPrefab.prefab";
        names[0] = "ArtResources/Prefabs/MyPrefab.prefab";

        build.assetBundleName = "myprefab";
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

        build = new AssetBundleBuild();
        assets = new string[1];
        assets[0] = "Assets/ArtResources/Prefabs/TestPrefab.prefab";
        build.assetBundleName = "testprefab";
        build.assetNames = assets;
        builds.Add(build);

        string outDir = Path.Combine(Application.dataPath, "../AssetBundles/Test");
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

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

    [MenuItem("Test/Test2")]
    public static void Test2()
    {
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        AssetBundleBuild build = new AssetBundleBuild();
        string[] assets = new string[1];
        string[] names = new string[1];
        assets[0] = "Assets/ArtResources/Prefabs/MyPrefab.prefab";
        names[0] = "ArtResources/Prefabs/MyPrefab.prefab";

        build.assetBundleName = "allAB";
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


        string outDir = Path.Combine(Application.dataPath, "../AssetBundles/Test2");
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

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


    [MenuItem("Test/Test3")]
    public static void Test3()
    {
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        AssetBundleBuild build = new AssetBundleBuild();
        string[] assets = new string[3];
        string[] names = new string[3];
        assets[0] = "Assets/ArtResources/Prefabs/MyPrefab.prefab";
        names[0] = "ArtResources/Prefabs/MyPrefab.prefab";
        assets[1] = "Assets/ArtResources/Materials/MyMaterial.mat";
        names[1] = "ArtResources/Materials/MyMaterial.mat";
        assets[2] = "Assets/ArtResources/Textures/bnt_Blue_S.png";
        names[2] = "ArtResources/Textures/bnt_Blue_S.png";

        build.assetBundleName = "allInOne";
        build.assetNames = assets;
        build.addressableNames = names;

        builds.Add(build);     


        string outDir = Path.Combine(Application.dataPath, "../AssetBundles/Test3");
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

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

    [MenuItem("Test/Test22")]
    public static void Test22()
    {
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        AssetBundleBuild build = new AssetBundleBuild();
        string[] assets = new string[1];
        string[] names = new string[1];
        assets[0] = "Assets/ArtResources/Prefabs/MyPrefab.prefab";
        names[0] = "ArtResources/Prefabs/MyPrefab.prefab";

        build.assetBundleName = "partOne2";
        build.assetNames = assets;
        build.addressableNames = names;

        builds.Add(build);

        //build = new AssetBundleBuild();
        //assets = new string[1];
        //assets[0] = "Assets/ArtResources/Materials/MyMaterial.mat";
        //build.assetBundleName = "bbbbbb";
        //build.assetNames = assets;
        //builds.Add(build);

        //build = new AssetBundleBuild();
        //assets = new string[1];
        //assets[0] = "Assets/ArtResources/Textures/bnt_Blue_S.png";
        //build.assetBundleName = "ccccc";
        //build.assetNames = assets;
        //builds.Add(build);


        string outDir = Path.Combine(Application.dataPath, "../AssetBundles/Test2");
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

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
        Debug.Log(Relative("c:\\a\\b\\c", "d:\\e\\f\\g"));
        Debug.Log(Relative("c:\\a\\b\\c", "c:\\a\\b\\c\\d\\e\\f"));
        Debug.Log(Relative("c:\\a\\b\\c", "c:\\a\\b\\d\\e\\f"));

        Debug.Log(Relative("/a/b/c", "/e/f/g"));
        Debug.Log(Relative("/a/b/c", "/a/b/c/d/e/f"));
        Debug.Log(Relative("/a/b/c", "/a/b/d/e/f"));
    }

    public static string Relative(string from,string to)
    {
        string[] froms = from.Replace("\\", "/").Split('/');
        string[] tos = to.Replace("\\", "/").Split('/');

        int i = 0;
        //look for same part
        for(; i < froms.Length; ++i)
        {
            if (froms[i] != tos[i])
            {
                break;
            }
        }

        if (i == 0)
        {
            //just windows. eg.from=c:\a\b\c,to=d:\e\f\g
            //if linux the first is empty always same. eg. from=/a/b/c,to=/d/e/f
            return to;
        }
        else
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();

            for (int j=i; j < froms.Length; ++j)
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
