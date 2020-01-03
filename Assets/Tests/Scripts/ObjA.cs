using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjA : MonoBehaviour
{
    bool m_Destoryed = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        m_Destoryed = true;
    }

    public void DoLoadAssetCallback(string asset)
    {
        Debug.LogFormat("Asset {0} is loaded,d:{1}", asset,m_Destoryed);
    }
}
