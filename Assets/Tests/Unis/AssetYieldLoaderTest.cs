using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using YH.AssetManage;

namespace Tests
{
	public class AssetYieldLoaderTest
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
			//每个test之前执行
		}

		[TearDown]
		public void TearDown()
		{
			//每个test之后执行
			m_AssetManager.UnloadUnuseds();
		}

		[UnityTest]
		public IEnumerator TestYeildLoadAsset()
		{
			AssetLoaderEnumerator assetLoaderEnumerator = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
			yield return assetLoaderEnumerator;
			AssetReference result = assetLoaderEnumerator.assetReference;
			Assert.AreNotEqual(result, null);
			assetLoaderEnumerator.Dispose();
		}

		[UnityTest]
		public IEnumerator TestYeildLoadSameAsset()
		{
			AssetLoaderEnumerator assetLoaderEnumerator = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
			yield return assetLoaderEnumerator;
			AssetReference result = assetLoaderEnumerator.assetReference;
			Assert.AreNotEqual(result, null);
			assetLoaderEnumerator.Dispose();

			assetLoaderEnumerator = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
			yield return assetLoaderEnumerator;
			result = assetLoaderEnumerator.assetReference;
			Assert.AreNotEqual(result, null);
			assetLoaderEnumerator.Dispose();
		}
	}
}