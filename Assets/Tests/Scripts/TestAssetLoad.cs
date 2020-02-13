using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManage;

public class TestOperation : IEnumerator,System.IDisposable
{

    public object Current
    {
        get
        {
            Debug.Log("#### current");
            return null;
        }
    }

    public bool MoveNext()
    {
        Debug.Log("#### MoveNext");
        return false;
    }

    public void Reset()
    {
        Debug.Log("#### Reset");
    }

    #region IDisposable Support
    private bool disposedValue = false; // 要检测冗余调用

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Debug.Log("# dispose 1");
                // TODO: 释放托管状态(托管对象)。
            }
            Debug.Log("# dispose 2");
            // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
            // TODO: 将大型字段设置为 null。

            disposedValue = true;
        }
    }

    // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
    // ~TestOperation() {
    //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
    //   Dispose(false);
    // }

    // 添加此代码以正确实现可处置模式。
    public void Dispose()
    {
        Debug.Log("# dispose");
        // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        Dispose(true);
        // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
        // GC.SuppressFinalize(this);
    }
    #endregion
}

public class TestAssetLoad : MonoBehaviour
{
    [SerializeField]
    AssetManager m_AssetManager;

    Object m_Obj;
    // Use this for initialization
    void Start()
    {
        YH.AssetsMonitor.Instance.CheckAssets();

        m_AssetManager.Init(null,(r) =>
        {
            StartCoroutine(Testloop2());
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        m_AssetManager.UnloadUnuseds();
    }

    IEnumerator Test()
    {
        yield return new WaitForSeconds(2);
        m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                Debug.Log(ar.asset);
                GameObject.Instantiate(ar.asset);
            }
        });

        m_AssetManager.LoadAsset("ArtResources/Materials/MyMaterial.mat", (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                Debug.Log(ar.asset);
                GameObject.Instantiate(ar.asset);
            }
        });

        m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                Debug.Log(ar.asset);
                GameObject.Instantiate(ar.asset);
            }
        });
        //Debug.Log("start " + Time.frameCount);
        //using(TestOperation tt= new TestOperation())
        //{
        //    yield return tt;
        //}
        //Debug.Log("end " + Time.frameCount);
    }

    IEnumerator Test2()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("Load prefab " + Time.frameCount);
        AssetLoaderEnumerator loader = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
         yield return loader;
        AssetReference ar = loader.assetReference;
        Debug.Log(ar + "," + Time.frameCount);
        if (ar != null)
        {
            Debug.Log(ar.asset);
            m_Obj = GameObject.Instantiate(ar.asset);
            //ar.Retain(m_Obj);
            ar.Monitor(m_Obj as GameObject);
        }
        loader.Dispose();

        Debug.Log("Load mat " + Time.frameCount);
        AssetLoaderEnumerator loader2 = m_AssetManager.YieldLoadAsset("ArtResources/Materials/MyMaterial.mat");
        ar = loader2.assetReference;
        Debug.Log(ar + "," + Time.frameCount);
        if (ar != null)
        {
            Debug.Log(ar.asset);
        }
        loader2.Dispose();
        Debug.Log("Load complete " + Time.frameCount);
        yield return new WaitForSeconds(2);
        Debug.Log("start dstroy " + Time.frameCount);
        if (m_Obj != null)
        {
            Destroy(m_Obj);
        }

        yield return null;
        m_AssetManager.UnloadUnuseds();

        //yield return m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
        //{

        //    Debug.Log(ar + "," + Time.frameCount);
        //    if (ar != null)
        //    {
        //        Debug.Log(ar.asset);
        //        GameObject go = GameObject.Instantiate(ar.asset) as GameObject;
        //        go.transform.Translate(10, 0, 0);
        //    }
        //});

    }

    IEnumerator Test3()
    {
        AssetLoaderEnumerator loader = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
        yield return loader;
        AssetReference assetRef = loader.assetReference;
        Debug.Log(Time.frameCount);
        //资源已经在LoadAsset时已经被加载出来，后面删除AssetBundle也没有关系。
        yield return new WaitForSeconds(1);
        Debug.Log("Remove bundle," + Time.frameCount);
        assetRef.assetBundleReference = null;

        yield return new WaitForSeconds(1);
        Debug.Log("Instance," + Time.frameCount);
        GameObject.Instantiate(assetRef.asset);
        loader.Dispose();
    }

    IEnumerator Test4()
    {
        AssetLoaderEnumerator loader = m_AssetManager.YieldLoadAsset("ArtResources/Materials/MyMaterial.mat");
        yield return loader;
        AssetReference ar = loader.assetReference;
        Debug.Log(ar + "," + Time.frameCount);
        if (ar != null)
        {
            Debug.Log(ar.asset);
            ar.assetBundleReference = null;
        }
        loader = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/MyPrefab.prefab");
        yield return loader;
        Debug.Log(ar + "," + Time.frameCount);
        if (ar != null)
        {
            GameObject.Instantiate(ar.asset);
        }
        loader.Dispose();
    }

    IEnumerator Testloop()
    {
        AssetLoaderEnumerator loader = m_AssetManager.YieldLoadAsset("ArtResources/Prefabs/APreab.prefab");
        yield return loader;
        AssetReference ar = loader.assetReference;
        Debug.Log(ar + "," + Time.frameCount);
        if (ar != null)
        {
            GameObject.Instantiate(ar.asset);
        }
        loader.Dispose();
    }

    IEnumerator Testloop2()
    {
        string[] assets = new string[] { "ArtResources/Materials/MyMaterial.mat", "ArtResources/Prefabs/MyPrefab.prefab" , "ArtResources/Prefabs/APreab.prefab" };
        for(int i = 0; i < assets.Length; ++i)
        {
            Debug.Log("load:" + assets[i]);
            int j = i;
            string path = assets[i];
            m_AssetManager.LoadAsset(path, (ar) =>
            {
                //0,3:ArtResources/Materials/MyMaterial.mat
                //1,3:ArtResources/Prefabs/MyPrefab.prefab
                //2,3:ArtResources/Prefabs/APreab.prefab
                Debug.Log(ar + ",:"+j+","+i+":"+ path);
                if (ar != null)
                {
                    GameObject.Instantiate(ar.asset);
                }
            });
        }
        yield return null;
    }
    
    IEnumerator Testloop3()
    {
        string[] assets = new string[] { "ArtResources/Materials/MyMaterial.mat", "ArtResources/Prefabs/MyPrefab.prefab" , "ArtResources/Prefabs/APreab.prefab" };
        foreach(var asset in assets)
        {
            Debug.Log("load:" + asset);
            string path = asset;
            m_AssetManager.LoadAsset(path, (ar) =>
            {
                //ArtResources/Materials/MyMaterial.mat:ArtResources/Materials/MyMaterial.mat
                //ArtResources/Prefabs/MyPrefab.prefab:ArtResources/Prefabs/MyPrefab.prefab
                //ArtResources/Prefabs/APreab.prefab:ArtResources/Prefabs/APreab.prefab
                Debug.Log(ar + ":"+path+","+asset);
                if (ar != null)
                {
                    GameObject.Instantiate(ar.asset);
                }
            });
        }
        yield return null;
    }
}
