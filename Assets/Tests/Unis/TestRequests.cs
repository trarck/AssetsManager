using System.Collections;
using System.Collections.Generic;
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
            BundleCreateSyncRequest request = new BundleCreateSyncRequest("myprefab",0);
            request.Start();
            AssetBundle result = request.assetBundle;
            Assert.AreNotEqual(result, null);
        }

        [UnityTest]
        public IEnumerator TestCreateAsyncRequest()
        {
            BundleCreateAsyncRequest request = new BundleCreateAsyncRequest("myprefab", 0);
            request.Start();
            yield return request;
            AssetBundle result = request.assetBundle;
            Assert.AreNotEqual(result, null);
        }

        //[UnityTest]
        //public IEnumerator TestAssetAsyncRequest()
        //{
        //    BundleCreateAsyncRequest request = new BundleCreateAsyncRequest("myprefab", 0);
        //    request.Start();
        //    yield return request;
        //    AssetBundle result = request.assetBundle;
        //    Assert.AreNotEqual(result, null);
        //}
    }
}
