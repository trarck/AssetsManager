using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using YH.AssetManage;

namespace Tests
{
	public class AssetBundleMissTest
	{
		AssetManager m_AssetManager;
		string m_AssetBundlePath = null;
		string m_NewAssetBundlePath = null;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Debug.unityLogger.filterLogType = LogType.Exception;

			m_AssetManager = AssetManager.Instance;
			m_AssetManager.Init();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Debug.unityLogger.filterLogType = LogType.Log;
			m_AssetManager.Clean();
		}

		[SetUp]
		public void SetUp()
		{
			IInfoManager infoManager = m_AssetManager.infoManager;
			AssetInfo assetInfo = infoManager.FindAssetInfo("ArtResources/Prefabs/MyPrefab.prefab");
			Assert.AreNotEqual(assetInfo, null);

			m_AssetBundlePath = AssetPaths.GetFullPath(assetInfo.bundleName);

			bool exists = System.IO.File.Exists(m_AssetBundlePath);
			Assert.IsTrue(exists);

			m_NewAssetBundlePath = m_AssetBundlePath + ".bak";
			System.IO.File.Move(m_AssetBundlePath, m_NewAssetBundlePath);
		}

		[TearDown]
		public void TearDown()
		{
			System.IO.File.Move(m_NewAssetBundlePath,m_AssetBundlePath);

			m_AssetManager.UnloadUnuseds();
		}


		[UnityTest]
		public IEnumerator TestAssetBundleMiss()
		{
			int loadCount = 1;
			AssetReference ar = null;
			m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (tar) =>
			{
				ar = tar;
				--loadCount;
			});
			while (loadCount > 0)
			{
				yield return null;
			}
			Assert.AreEqual(ar, null);
		}
	}
}