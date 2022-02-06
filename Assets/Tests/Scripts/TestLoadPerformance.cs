using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManage;

public class TestLoadPerformance : MonoBehaviour
{
    [SerializeField]
    AssetManager m_AssetManager;

    // Start is called before the first frame update
    void Start()
    {
        m_AssetManager.Init(null, (r) =>
        {
            DoTest();
        });
    }

    // Update is called once per frame
    void DoTest()
    {
        
    }
}
