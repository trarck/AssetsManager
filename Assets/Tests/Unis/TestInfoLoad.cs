using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YH.AssetManage;

namespace Tests
{
    public class TestInfoLoad
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("OneTimeSetUp");
            AssetPaths.SetupDefaultSearchPaths();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Debug.Log("OneTimeTearDown");
        }

        [SetUp]
        public void SetUp()
        {
            Debug.Log("Setup");
        }

        [TearDown]
        public void TearDown()
        {
            Debug.Log("TearDown");
        }
        
        [Test]
        public void TestAssetBundleManifestLoad()
        {
            InfoManager infoManager = new InfoManager(null);
            infoManager.Init();
            string manifestFullPath = AssetPaths.GetFullPath(AssetPaths.bundleManifestFile);
            infoManager.Load(manifestFullPath);

            string assetPath = "Assets/ArtResources/Prefabs/MyPrefab.prefab";
            AssetLoadInfo assetLoadInfo = infoManager.GetAssetInfo(assetPath);
            Assert.AreNotEqual(assetLoadInfo, null);
            AssetBundleLoadInfo assetBundleLoadInfo = infoManager.GetAssetBundleInfo(assetLoadInfo.bunldeId);
            Assert.AreNotEqual(assetLoadInfo, null);
        }
    }
}
