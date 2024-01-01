//using System.Collections;
//using NUnit.Framework;
//using UnityEngine;
//using UnityEngine.TestTools;
//using YH.AssetManage;

//namespace Tests
//{
//    public class TestRemote
//    {
//        AssetManager m_AssetManager;

//        [OneTimeSetUp]
//        public void OneTimeSetUp()
//        {
//            Debug.Log("OneTimeSetUp");
//            AssetPaths.remoteUrl = "http://localhost:8012/StandaloneWindows64/";
//            m_AssetManager = AssetManager.Instance;
//            m_AssetManager.Init();
//        }

//        [OneTimeTearDown]
//        public void OneTimeTearDown()
//        {
//            Debug.Log("OneTimeTearDown");
//            m_AssetManager.Clean();
//        }

//        [SetUp]
//        public void SetUp()
//        {
//            Debug.Log("Setup");
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            Debug.Log("TearDown");

//            //m_AssetManager.UnloadUnuseds();
//        }

//        #region Asset

//        [UnityTest]
//        public IEnumerator TestLoadAsset()
//        {
//            AssetReference result = null;
//            bool isDone = false;
//            m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
//            {
//                isDone = true;
//                result = ar;
//            });

//            while (!isDone)
//            {
//                yield return null;
//            }
//            Assert.AreNotEqual(result, null);
//        }

//        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
//        // `yield return null;` to skip a frame.
//        [UnityTest]
//        public IEnumerator TestYeildLoadAsset()
//        {
//            AssetLoaderEnumerator assetLoaderEnumerator = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
//            yield return assetLoaderEnumerator;
//            AssetReference result = assetLoaderEnumerator.assetReference;
//            Assert.AreNotEqual(result, null);
//            assetLoaderEnumerator.Dispose();
//        }

//        [UnityTest]
//        public IEnumerator TestYeildLoadAsset2()
//        {
//            AssetLoaderEnumerator assetLoaderEnumerator = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
//            yield return assetLoaderEnumerator;
//            AssetReference result = assetLoaderEnumerator.assetReference;
//            Assert.AreNotEqual(result, null);
//            assetLoaderEnumerator.Dispose();

//            assetLoaderEnumerator = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
//            yield return assetLoaderEnumerator;
//            result = assetLoaderEnumerator.assetReference;
//            Assert.AreNotEqual(result, null);
//            assetLoaderEnumerator.Dispose();
//        }

//        #endregion

//        #region AssetBundle

//        [UnityTest]
//        public IEnumerator TestLoadAssetBundle()
//        {
//            AssetBundleReference result = null;
//            bool isDone = false;
//            m_AssetManager.LoadAssetBundle("prefabs/myprefab", false, (abr) =>
//            {
//                isDone = true;
//                result = abr;
//            });

//            while (!isDone)
//            {
//                yield return null;
//            }
//            Assert.AreNotEqual(result, null);
//        }

//        [UnityTest]
//        public IEnumerator TestYeildLoadAssetBundle()
//        {
//            BundleLoaderEnumerator bundleLoaderEnumerator = m_AssetManager.YieldLoadAssetBundle("prefabs/myprefab", false);
//            yield return bundleLoaderEnumerator;
//            AssetBundleReference result = bundleLoaderEnumerator.assetBundleReference;
//            Assert.AreNotEqual(result, null);
//            bundleLoaderEnumerator.Dispose();
//        }

//        #endregion
//    }
//}
