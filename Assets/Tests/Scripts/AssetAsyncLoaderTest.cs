using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using YH.AssetManage;

public class AssetAsyncLoaderTest
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
	public IEnumerator SimpleLoader()
    {
        AssetReference result = null;

        yield return m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
        {
            Debug.Log("ddddd");
            Debug.Log(ar);
            result = ar;
        });
        Debug.Log("bbbb,"+Time.frameCount);
        Assert.AreNotEqual(result, null);

        yield return m_AssetManager.LoadAsset("ArtResources/Materials/MyMaterial.mat", (ar) =>
        {
            result = ar;
        });
        Assert.AreNotEqual(result, null);

        yield return m_AssetManager.LoadAsset("ArtResources/Textures/bnt_Blue_S.png", (ar) =>
        {
            result = ar;
        });
        Assert.AreNotEqual(result, null);
    }

    [UnityTest]
    public IEnumerator LoadDuplicate()
    {
        AssetReference result1 = null;
        AssetReference result2 = null;

        m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
        {
            result1 = ar;
        });

        yield return m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
        {
            result2 = ar;
        });

        Assert.AreEqual(result1, result2);

    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator LoaderAndDestroy()
    {
        Object obj = null;

        yield return m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
        {
            Debug.Log("BBB," + Time.frameCount);
            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                Debug.Log(ar.asset);
                obj = GameObject.Instantiate(ar.asset);
                //ar.Retain(m_Obj);
                ar.Monitor(obj as GameObject);
            }
        });
        Debug.Log("DDD,"+Time.frameCount);
        Assert.AreEqual(1, m_AssetManager.assets.Count);
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
        Assert.AreEqual(3, m_AssetManager.assetBundles.Count);
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

        Assert.AreEqual(0,m_AssetManager.assets.Count);

        Assert.AreEqual(0,m_AssetManager.assetBundles.Count);
    }

    [Test]
    public void SyncLoad()
    {
        AssetReference ar = m_AssetManager.LoadAssetSync("ArtResources/Prefabs/MyPrefab.prefab");
        Assert.AreNotEqual(ar, null);

        AssetBundleReference abr = m_AssetManager.LoadAssetBundleSync("mymaterial");
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
        Assert.AreNotEqual(abr, null);
#else
        Assert.AreEqual(abr, null);
#endif
    }

}
