using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YH.AssetManage;

namespace Tests
{
    public class TestRequests
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
        public void TestSyncRequest()
        {
            string assetbundlePath = AssetPaths.GetFullPath("myprefab"+AssetPaths.AssetBundelExt);
            BundleCreateSyncRequest request = new BundleCreateSyncRequest(assetbundlePath, 0);
            request.Start();
            AssetBundle result = request.assetBundle;
            Assert.AreNotEqual(result, null);
        }

        [UnityTest]
        public IEnumerator TestCreateAsyncRequest()
        {
            string assetbundlePath = AssetPaths.GetFullPath("myprefab" + AssetPaths.AssetBundelExt);
            BundleCreateAsyncRequest request = new BundleCreateAsyncRequest(assetbundlePath, 0);
            request.Start();
            yield return request;
            AssetBundle result = request.assetBundle;
            Assert.AreNotEqual(result, null);
        }

        [UnityTest]
        public IEnumerator TestAssetAsyncRequest()
        {
            string assetbundlePath = AssetPaths.GetFullPath("myprefab" + AssetPaths.AssetBundelExt);
            BundleCreateAsyncRequest request = new BundleCreateAsyncRequest(assetbundlePath, 0);
            request.Start();
            yield return request;
            AssetBundle result = request.assetBundle;
            Assert.AreNotEqual(result, null);
            AssetLoaderAsyncRequest assetRequest = new AssetLoaderAsyncRequest(result, "Assets/ArtResources/Prefabs/MyPrefab.prefab", typeof(GameObject));
            assetRequest.Start();
            yield return assetRequest;
            GameObject asset = assetRequest.asset as GameObject;
            Assert.AreNotEqual(asset, null);
        }

        [UnityTest]
        public IEnumerator TestAssetResourceRequest()
        {
            ResouceLoaderRequest assetRequest = new ResouceLoaderRequest("Tests/ResPrefab", typeof(GameObject));

            string assetbundlePath = AssetPaths.GetFullPath("myprefab" + AssetPaths.AssetBundelExt);
            BundleCreateAsyncRequest request = new BundleCreateAsyncRequest(assetbundlePath, 0);
            request.Start();
            yield return request;
            AssetBundle result = request.assetBundle;
            Assert.AreNotEqual(result, null);
        }
    }
}
