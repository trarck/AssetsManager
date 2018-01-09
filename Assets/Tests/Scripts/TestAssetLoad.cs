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

        m_AssetManager.Init(()=>{

            StartCoroutine(Test2());

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

        //yield return m_AssetManager.LoadAsset("ArtResources/Materials/MyMaterial.mat", (ar) =>
        //{
        //    Debug.Log(ar + "," + Time.frameCount);
        //    if (ar != null)
        //    {
        //        Debug.Log(ar.asset);
        //    }
        //});

        //yield return m_AssetManager.LoadAsset("ArtResources/Prefabs/MyPrefab.prefab", (ar) =>
        //{

        //    Debug.Log(ar + "," + Time.frameCount);
        //    if (ar != null)
        //    {
        //        Debug.Log(ar.asset);
        //        GameObject.Instantiate(ar.asset);
        //    }
        //});


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
    }
}
