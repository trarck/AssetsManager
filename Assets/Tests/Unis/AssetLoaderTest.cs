using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using YH.AssetManage;

namespace Tests
{
	public class AssetLoaderTest
	{
		AssetManager m_AssetManager;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			Debug.Log("OneTimeSetUp");
			m_AssetManager = AssetManager.Instance;
			m_AssetManager.Init();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			Debug.Log("OneTimeTearDown");
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

			m_AssetManager.UnloadUnuseds();
		}

		[UnityTest]
		public IEnumerator TestSimpleLoader()
		{
			AssetReference result1 = null;
			AssetReference result2 = null;
			AssetReference result3 = null;

			int loadCount = 3;

			m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
			{
				result1 = ar;
				--loadCount;
			});

			m_AssetManager.LoadAsset("ArtResources/Materials/MyMaterial.mat", (ar) =>
			{
				result2 = ar;
				--loadCount;
			});
			m_AssetManager.LoadAsset("ArtResources/Textures/bnt_Blue_S.png", (ar) =>
			{
				result3 = ar;
				--loadCount;
			});

			while (loadCount > 0)
			{
				yield return null;
			}

			Assert.AreNotEqual(result1, null);
			Assert.AreNotEqual(result2, null);
			Assert.AreNotEqual(result3, null);
		}

		[UnityTest]
		public IEnumerator TestLoadDuplicate()
		{
			AssetReference result1 = null;
			AssetReference result2 = null;
			int loadCount = 2;

			m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
			{
				result1 = ar;
				--loadCount;
			});

			m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
			{
				result2 = ar;
				--loadCount;

			});

			while (loadCount > 0)
			{
				yield return null;
			}
			Assert.AreEqual(result1, result2);
		}

		// A UnityTest behaves like a coroutine in PlayMode
		// and allows you to yield null to skip a frame in EditMode
		[UnityTest]
		public IEnumerator TestLoaderAndDestroy()
		{
			int loadCount = 1;

			Object obj = null;

			m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
			{
				--loadCount;
				if (ar != null)
				{
					obj = GameObject.Instantiate(ar.asset);
					//ar.Retain(m_Obj);
					ar.Monitor(obj as GameObject);
				}
			}, false);

			while (loadCount > 0)
			{
				yield return null;
			}

			Assert.AreEqual(1, m_AssetManager.assets.Count);
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
			Assert.AreEqual(2, m_AssetManager.assetBundles.Count);
#else
        Assert.AreEqual(0,m_AssetManager.assetBundles.Count);
#endif

			yield return new WaitForSeconds(0.2f);

			if (obj != null)
			{
				UnityEngine.Object.Destroy(obj);
			}

			yield return null;

			m_AssetManager.UnloadUnuseds();

			Assert.AreEqual(0, m_AssetManager.assets.Count);

			Assert.AreEqual(0, m_AssetManager.assetBundles.Count);
		}

		[Test]
		public void TestSyncLoad()
		{
			AssetReference ar = m_AssetManager.LoadAssetSync("ArtResources/Prefabs/MyPrefab.prefab");
			Assert.AreNotEqual(ar, null);

			AssetReference ar2 = m_AssetManager.LoadAssetSync("ArtResources / Materials / MyMaterial.mat");
			Assert.AreNotEqual(ar2, null);
		}

		[UnityTest]
		public IEnumerator TestSyncThenAsyncLoad()
		{
			//先同步再异步
			AssetReference ar = m_AssetManager.LoadAssetSync("ArtResources/Prefabs/MyPrefab.prefab");
			Assert.AreNotEqual(ar, null);

			int loadCount = 1;
			AssetReference ar2 = null;
			m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (tar) =>
			 {
				 ar2 = tar;
				 --loadCount;
			 });
			while (loadCount > 0)
			{
				yield return null;
			}
			Assert.AreNotEqual(ar2, null);
		}

		[UnityTest]
		public IEnumerator TestAsyncThenSyncLoad()
		{
			//先异步再同步
			int loadCount = 1;
			AssetReference ar2 = null;
			m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (tar) =>
			{
				ar2 = tar;
				--loadCount;
			});
			while (loadCount > 0)
			{
				yield return null;
			}
			Assert.AreNotEqual(ar2, null);

			AssetReference ar = m_AssetManager.LoadAssetSync("ArtResources/Prefabs/MyPrefab.prefab");
			Assert.AreNotEqual(ar, null);
		}

		[UnityTest]
		public IEnumerator TestNotExists()
		{
			//要关闭错误日志，否则携程会中断。
			Debug.unityLogger.filterLogType = LogType.Exception;
			AssetReference result1 = null;

			int loadCount = 1;

			m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefabXXX.prefab", (ar) =>
			{
				result1 = ar;
				--loadCount;
			});

			while (loadCount > 0)
			{
				yield return null;
			}

			Assert.AreEqual(result1, null);
			Debug.unityLogger.filterLogType = LogType.Log;
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