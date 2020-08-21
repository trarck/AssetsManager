using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestResource : MonoBehaviour {

    [SerializeField]
    GameObject m_Target;

    float m_Elapsed;
    int sw = 1;
    [SerializeField]
    Object m_O;
    AssetBundle m_AssetBundle;
	// Use this for initialization
	void Start ()
    {
        m_O = null;
        YH.AssetsMonitor.Instance.CheckAssets();
		StartCoroutine(Test());
        //List<YH.AssetsMonitor.AssetItem> assets = YH.AssetsMonitor.Instance.assets;
        //for (int i = 0, l = assets.Count; i < l; ++i)
        //{
        //    Debug.LogFormat("asset name={0},type={1},id={2}", assets[i].name, assets[i].type, assets[i].instanceId);
        //}
        
    }

	IEnumerator Test()
	{
		var assetBundleFile = Path.Combine(Application.dataPath, "../AssetBundles/StandaloneWindows64/prefabs/myprefab");
		var req = AssetBundle.LoadFromFileAsync(assetBundleFile);
		Debug.Log(req.assetBundle);
		var req2 = AssetBundle.LoadFromFileAsync(assetBundleFile);
		//Debug.Log(req2.assetBundle);
		yield return req;
		
		Debug.Log(req.assetBundle);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
