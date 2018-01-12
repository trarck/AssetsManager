using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManager;
public class TestAssetLoad : MonoBehaviour {
    [SerializeField]
    AssetManager m_AssetManager;

    Object m_Obj;
	// Use this for initialization
	void Start ()
    {
        YH.AssetsMonitor.Instance.CheckAssets();

        Object o=Resources.Load("Main");
        Debug.Log(o+","+ o.GetType() );

        m_AssetManager.Init((r)=>{
            StartCoroutine(Test3());

        });
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void OnDestroy()
    {
        m_AssetManager.UnloadUnuseds();
    }

    IEnumerator Test()
    {
        yield return new WaitForSeconds(2);
        m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab",(ar)=> {

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
    }

    IEnumerator Test2()
    {
        yield return new WaitForSeconds(2);

        yield return m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
        {

            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                Debug.Log(ar.asset);
                m_Obj = GameObject.Instantiate(ar.asset);
                //ar.Retain(m_Obj);
                ar.Monitor(m_Obj as GameObject);
            }
        });

        yield return m_AssetManager.LoadAsset("ArtResources/Materials/MyMaterial.mat", (ar) =>
        {
            Debug.Log(ar + "," + Time.frameCount);
            if (ar != null)
            {
                Debug.Log(ar.asset);
            }
        });


        //yield return m_AssetManager.LoadAsset("ArtResources/Materials/MyMaterial.mat", (ar) =>
        //{

        //    Debug.Log(ar + "," + Time.frameCount);
        //    if (ar != null)
        //    {
        //        Debug.Log(ar.asset);
        //    }
        //});

        Debug.Log("Load complete "+Time.frameCount);
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

        yield return m_AssetManager.LoadAssetBundle("myprefab","My",true,(abr) =>
        {
            Debug.Log(abr + "," + Time.frameCount);
        });

        yield return m_AssetManager.LoadAssetBundle("myprefab", "They", true, (abr) =>
        {
            Debug.Log(abr + "," + Time.frameCount);
        });

        Debug.Log("UnloadUnused " + Time.frameCount);

        m_AssetManager.UnloadUnuseds("My");
        yield return new WaitForSeconds(1);

        m_AssetManager.UnloadUnuseds("They");
    }
}
