using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YH.AssetManage;

namespace Tests
{
    public class TestCache
    {
        AssetManager m_AssetManager;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("OneTimeSetUp");
            AssetPaths.remoteUrl = "http://localhost:8012/StandaloneWindows64/";
            m_AssetManager = AssetManager.Instance;
            m_AssetManager.Init();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Debug.Log("OneTimeTearDown");
#if ASSETMANAGE_BUNDLE_CACHE_ON
            //save cache info
            RequestManager requestManager = m_AssetManager.requestManager as RequestManager;
            if (requestManager!=null && requestManager.cacheManager != null)
            {
                requestManager.cacheManager.SaveCacheInfo();
            }
#endif
            m_AssetManager.Clean();
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

            //m_AssetManager.UnloadUnuseds();
        }

        #region AssetBundle

        [UnityTest]
        public IEnumerator TestLoadAssetBundle()
        {
            string bundleName = "prefabs/myprefab";
            AssetBundleReference result = null;

            bool isDone = false;
            m_AssetManager.LoadAssetBundle(bundleName, false, (abr) =>
            {
                isDone = true;
                result = abr;
            });

            while (!isDone)
            {
                yield return null;
            }
            Assert.AreNotEqual(result, null);
#if ASSETMANAGE_BUNDLE_CACHE_ON
            //check file Exists
            string filepath = AssetPaths.FullPathForFilename(bundleName);
            Assert.AreEqual(System.IO.File.Exists(filepath), true);
            //check cache info
            RequestManager rm = m_AssetManager.requestManager as RequestManager;
            if(rm!=null && rm.cacheManager != null)
            {
                Assert.AreNotEqual(rm.cacheManager.cacheItems, null);
                CacheItem cacheItem = null;
                if(rm.cacheManager.cacheItems.TryGetValue(bundleName,out cacheItem))
                {
                    Assert.AreEqual(string.IsNullOrEmpty(cacheItem.hash),false);
                }
                else
                {
                    Assert.Fail("Can't find cache item "+bundleName);
                }
            }
#endif
        }

        [UnityTest]
        public IEnumerator TestYeildLoadAssetBundle()
        {
            string bundleName = "prefabs/myprefab";
            BundleLoaderEnumerator bundleLoaderEnumerator = m_AssetManager.YieldLoadAssetBundle(bundleName, false);
            yield return bundleLoaderEnumerator;
            AssetBundleReference result = bundleLoaderEnumerator.assetBundleReference;
            Assert.AreNotEqual(result, null);
#if ASSETMANAGE_BUNDLE_CACHE_ON
            //check file Exists
            string filepath = AssetPaths.FullPathForFilename(bundleName);
            Assert.AreEqual(System.IO.File.Exists(filepath), true);
            //check cache info
            RequestManager rm = m_AssetManager.requestManager as RequestManager;
            if (rm != null && rm.cacheManager != null)
            {
                Assert.AreNotEqual(rm.cacheManager.cacheItems, null);
                CacheItem cacheItem = null;
                if (rm.cacheManager.cacheItems.TryGetValue(bundleName, out cacheItem))
                {
                    Assert.AreEqual(string.IsNullOrEmpty(cacheItem.hash), false);
                }
                else
                {
                    Assert.Fail("Can't find cache item " + bundleName);
                }
            }
#endif
            bundleLoaderEnumerator.Dispose();
        }

        #endregion
    }
}
