using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManage;
using YH.AssetManage.Extension;

public class TestAssetLoad1 : MonoBehaviour
{
    [SerializeField]
    AssetManager m_AssetManager;

    [SerializeField]
    ObjA m_Obj;
    // Use this for initialization
    void Start()
    {
        YH.AssetsMonitor.Instance.CheckAssets();

        m_AssetManager.Init(null,(r) =>
        {
            DoTest();
        });
    }

    void DoTest()
    {
        Test1();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        m_AssetManager.UnloadUnuseds();
    }

    void Test1()
    {
        string asset1 = "ArtResources/Prefabs/MyPrefab.prefab";
        m_AssetManager.LoadAsset(asset1, (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                //if (m_Obj)
                {
                    m_Obj.DoLoadAssetCallback(asset1);
                }
            }
        });
        GameObject.Destroy(m_Obj.gameObject);
    }

    void Test2()
    {
        string asset1 = "ArtResources/Prefabs/MyPrefab.prefab";
        MonoAssetLoader.Create(m_Obj.gameObject).LoadAsset(asset1, (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                //if (m_Obj)
                {
                    m_Obj.DoLoadAssetCallback(asset1);
                }
            }
        });
        GameObject.Destroy(m_Obj.gameObject);
    }

    void Test3()
    {
        string asset1 = "ArtResources/Prefabs/MyPrefab.prefab";
        MonoAssetLoader.Create().LoadAsset(asset1, (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                //if (m_Obj)
                {
                    m_Obj.DoLoadAssetCallback(asset1);
                }
            }
        });
        GameObject.Destroy(m_Obj.gameObject);
    }

    void Test4()
    {
        string asset1 = "ArtResources/Prefabs/MyPrefab.prefab";
        Context c = ContextFactroy.GetMonoEventContext(m_Obj.gameObject);
        ContextAssetLoader.Create(c).LoadAsset(asset1, (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                //if (m_Obj)
                {
                    m_Obj.DoLoadAssetCallback(asset1);
                }
            }
        });
        GameObject.Destroy(m_Obj.gameObject);
    }

    void Test5()
    {
        string asset1 = "ArtResources/Prefabs/MyPrefab.prefab";
        MonoContext c = ContextFactroy.GetMonoContext(m_Obj.gameObject);
        ContextAssetLoader.Create(c).LoadAsset(asset1, (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                //if (m_Obj)
                {
                    m_Obj.DoLoadAssetCallback(asset1);
                }
            }
        });
        GameObject.Destroy(m_Obj.gameObject);
    }

    void Test6()
    {
        string asset1 = "ArtResources/Prefabs/MyPrefab.prefab";
        MonoContext c = ContextFactroy.GetMonoContext(m_Obj.gameObject);
        ContextAssetLoader.Create(c).LoadAsset(asset1, (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                //if (m_Obj)
                {
                    m_Obj.DoLoadAssetCallback(asset1);
                }
            }
        });
        c.enable = false;
        GameObject.Destroy(m_Obj.gameObject);
    }
}
