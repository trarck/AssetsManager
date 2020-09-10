using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YH.AssetManage;

namespace Tests
{
    public class AssetBundleLoaderTest
	{
        AssetManager m_AssetManager;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
			m_AssetManager = AssetManager.Instance;
            m_AssetManager.Init();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_AssetManager.Clean();
        }

        [SetUp]
        public void SetUp()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            m_AssetManager.UnloadUnuseds();
        }
	  
		[Test]
        public void TestLoadAssetBundleSync()
        {
            AssetBundleReference result = m_AssetManager.LoadAssetBundleSync("prefabs/myprefab", false);
            Assert.AreNotEqual(result, null);
            result.Release();
        }


        [UnityTest]
        public IEnumerator TestLoadAssetBundle()
        {
            AssetBundleReference result = null;
            bool isDone = false;
            m_AssetManager.LoadAssetBundle("prefabs/myprefab", false, (abr) =>
            {
                isDone = true;
                result = abr;
            });

            while (!isDone)
            {
                yield return null;
            }
            Assert.AreNotEqual(result, null);
        }

        [UnityTest]
        public IEnumerator TestYeildLoadAssetBundle()
        {
            BundleLoaderEnumerator bundleLoaderEnumerator = m_AssetManager.YieldLoadAssetBundle("prefabs/myprefab", false);
            yield return bundleLoaderEnumerator;
            AssetBundleReference result = bundleLoaderEnumerator.assetBundleReference;
            Assert.AreNotEqual(result, null);
            bundleLoaderEnumerator.Dispose();
        }

		[UnityTest]
		public IEnumerator TestLoadNotExists()
		{
			//要关闭错误日志，否则携程会中断。
			Debug.unityLogger.filterLogType = LogType.Exception;
			AssetBundleReference result = null;
			bool isDone = false;
			m_AssetManager.LoadAssetBundle("prefabs/myprefabXXX", false, (abr) =>
			{
				isDone = true;
				result = abr;
			});

			while (!isDone)
			{
				yield return null;
			}
			Assert.AreEqual(result, null);
			Debug.unityLogger.filterLogType = LogType.Log;
		}
	}
}
