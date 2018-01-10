using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine.AssetBundles.AssetBundleDataSource;

namespace UnityEngine.AssetBundles
{
    public class AssetBundleConstans
    {
        public static string MainSetting = "Library/AssetBundleBrowserMain.dat";
        public static string BuildTabSetting = "Library/AssetBundleBrowserBuild.dat";
        public static string InspectTabSetting = "Library/AssetBundleBrowserInspect.dat";
        public static string JsonDatabaseFile = "AssetDatabase/AssetDatabase.json";
    }
}