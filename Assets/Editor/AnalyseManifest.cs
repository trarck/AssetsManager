using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


public class AnalyseManifest
{

    [MenuItem("Test/AnalyseManifest")]
    public static void TestAnalyseManifest()
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "../AssetBundles/AllResources"));
        AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        string[] bundles = manifest.GetAllAssetBundles();
    }
}
