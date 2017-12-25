using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManager;
public class TestAssetLoad : MonoBehaviour {
    [SerializeField]
    AssetManager m_AssetManager;

	// Use this for initialization
	void Start ()
    {
        m_AssetManager.Init(()=>{

            StartCoroutine(Test());

        });
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    IEnumerator Test()
    {
        yield return new WaitForSeconds(2);
        m_AssetManager.LoadAsset("Assets/ArtResources/Prefabs/MyPrefab.prefab",(ar)=> {

            Debug.Log(ar);
            if (ar != null)
            {
                Debug.Log(ar.asset);
                GameObject.Instantiate(ar.asset);
            }
        });
    }
}
