using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManage;
using System;

public class TestLoadPerformance : MonoBehaviour
{
    [SerializeField]
    AssetManager m_AssetManager;

    // Start is called before the first frame update
    void Start()
    {
        m_AssetManager.Init(null, (r) =>
        {
        });
    }

    void OnGUI()
    {
        if (GUILayout.Button("AllInOne"))
        {
            TestAllInOne();
        }

        if (GUILayout.Button("SingleAll"))
        {
            TestAllInOne();
        }
    }

    // Update is called once per frame
    void DoTest()
    {
        TestSingleAll();
    }

    void TestAllInOne()
    {
        string allInOne = "bigprefab_allInOne.ab";
        DateTime start = DateTime.Now;
        
        m_AssetManager.LoadAsset<GameObject>(allInOne,(ar)=>
        {
            DateTime end = DateTime.Now;
            TimeSpan used = end - start;
            Debug.LogFormat("used:{0}", used.TotalMilliseconds);
            if (ar!=null)
            {
                Debug.Log(ar);
            }
        });
    }

    void TestSingleAll()
    {
        string allInOne = "bigprefab_singleAll.ab";
        DateTime start = DateTime.Now;

        m_AssetManager.LoadAsset<GameObject>(allInOne, (ar) =>
        {
            DateTime end = DateTime.Now;
            TimeSpan used = end - start;
            Debug.LogFormat("used:{0}", used.TotalMilliseconds);
            if (ar != null)
            {
                Debug.Log(ar);
            }
        });
    }
}
