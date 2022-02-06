using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManage;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour {

	// Use this for initialization
	void Start () {
        AssetManager.Instance.Init(null,(result) =>
        {
            GotoMain();
        });
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void GotoMain()
    {
        StartCoroutine(Testloop());
        //AssetManager.Instance.LoadAsset("Scenes/Main.unity", (ar) =>
        //{
        //    Debug.Log("LoadCompete");
        //    Debug.Log(ar.asset);
        //    SceneManager.LoadScene("Scenes/Main");
        //});

        //AssetManager.Instance.LoadScene("Scenes/Main.unity",AssetTag.Main, (abr) =>
        //{
        //    Debug.Log("LoadCompete");
        //    SceneManager.LoadScene("Scenes/Main");
        //});
    }

    IEnumerator Testloop()
    {
        AssetLoaderEnumerator loader = AssetManager.Instance.YieldLoadAsset("ArtResources/Prefabs/TestMatProp.prefab");
        yield return loader;
        AssetReference ar = loader.assetReference;
        Debug.Log(ar + "," + Time.frameCount);
        if (ar != null)
        {
            GameObject.Instantiate(ar.asset);
        }
        loader.Dispose();
    }

}
