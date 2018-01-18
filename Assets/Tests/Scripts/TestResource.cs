using System.Collections;
using System.Collections.Generic;
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
        //List<YH.AssetsMonitor.AssetItem> assets = YH.AssetsMonitor.Instance.assets;
        //for (int i = 0, l = assets.Count; i < l; ++i)
        //{
        //    Debug.LogFormat("asset name={0},type={1},id={2}", assets[i].name, assets[i].type, assets[i].instanceId);
        //}
        
    }

    // Update is called once per frame
    void Update()
    {
        m_Elapsed += Time.deltaTime;

        if (m_Elapsed > 1 && sw == 1)
        {
            sw = 2;
            m_AssetBundle=AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/bnt_blue_s");
        }

        if (m_Elapsed > 5 && sw == 2)
        {
            sw = 3;
            Texture2D texture= m_AssetBundle.LoadAsset("bnt_Blue_S2") as Texture2D;
            m_AssetBundle.LoadAsset<Texture2D>("bnt_blue");
            // MeshRenderer mesh = m_Target.GetComponent<MeshRenderer>();
            //mesh.material.mainTexture = texture;
            m_O = texture;
        }

        if (m_Elapsed > 6 && sw == 3)
        {
            sw = 4;
            //Resources.UnloadAsset(m_O);
            m_O = null;
            Resources.UnloadUnusedAssets();
            //m_AssetBundle.Unload(false);
            //m_AssetBundle = null;
            //Resources.UnloadUnusedAssets();
            //m_AssetBundle.Unload(false);
        }


        if (m_Elapsed > 8 && sw == 4)
        {
            sw = 5;
           //m_AssetBundle.Unload(true);
        }
    }
}
