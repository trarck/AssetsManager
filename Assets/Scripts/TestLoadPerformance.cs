using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YH.AssetManage;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using System.Text;

public class TestLoadPerformance : MonoBehaviour
{

    [SerializeField]
    InputField m_BundlePathInputField;

    [SerializeField]
    InputField m_AssetPathInputField;

    [SerializeField]
    Text m_Message;

    [SerializeField]
    Text m_MemValue;
    float m_Duration=0.5f;
    float m_Elapsed = 0;

    AssetManager m_AssetManagerOne;
    AssetManager m_AssetManagerMulti;
    AssetManager m_AssetManager;

    AssetBundle m_AssetBundle;

    private void Awake()
    {
        m_AssetManagerOne = new AssetManager();
        m_AssetManagerMulti = new AssetManager();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Application.streamingAssetsPath);
        m_AssetManagerOne.Init("allInOne.manifest.json", (r) =>
        {
            AddMessage("Init allinone " + (r ? "success" : "fail"));
        });

        m_AssetManagerMulti.Init(null, (r) =>
        {
            AddMessage("Init multi " + (r ? "success" : "fail"));
        });


        m_AssetManager = m_AssetManagerOne;
    }

    private void Update()
    {
        //if (m_AssetManager)
        {
            m_AssetManager.Update();
        }

         m_Elapsed +=Time.deltaTime;
        if(m_Elapsed > m_Duration)
        {
            m_Elapsed = 0;
            if (m_MemValue != null)
            {
                SetPropertySize(m_MemValue, RPGTools.Device.instance.GetMemoryAppSize());
            }
        }
    }

    public void OnAllinOneToggleChange(bool value)
    {
        if (value)
        {
            m_AssetManager = m_AssetManagerOne;
        }
    }

    public void OnMultiToggleChange(bool value)
    {
        if (value)
        {
            m_AssetManager = m_AssetManagerMulti;
        }
    }

    public void LoadBundleSync()
    {
        string bundlePath = m_BundlePathInputField.text;
        if (string.IsNullOrEmpty(bundlePath))
        {
            AddMessage("asset bunde path is empty.");
            return;
        }

        long startMem = RPGTools.Device.instance.GetMemoryAppSize();
        //AddMessage(String.Format("Before LoadBundleSync Mem:{0}", startMem));
        float startTime = Time.realtimeSinceStartup;
        var abr = m_AssetManager.LoadAssetBundleSync(bundlePath);
        float loadTime = Time.realtimeSinceStartup - startTime;
        long endMem = RPGTools.Device.instance.GetMemoryAppSize();
        //AddMessage(String.Format("After LoadBundleSync Mem:{0}", endMem));

        AddMessage("sync load bundle used:" + loadTime.ToString());
        AddMessage("sync load bundle mem change:" + (endMem-startMem));
        if (abr != null)
        {
            AddMessage("load bunde success.");
            m_AssetBundle = abr.assetBundle;
        }
        else
        {
            AddMessage("load bunde fail.");
            m_AssetBundle = null;
        }
    }

    public void LoadBundleASync()
    {
        string bundlePath = m_BundlePathInputField.text;
        if (string.IsNullOrEmpty(bundlePath))
        {
            AddMessage("asset bunde path is empty.");
            return;
        }

        long startMem = RPGTools.Device.instance.GetMemoryAppSize();
        float startTime = Time.realtimeSinceStartup;
        m_AssetManager.LoadAssetBundle(bundlePath, true, (abr) =>
        {
            float loadTime = Time.realtimeSinceStartup - startTime;
            long endMem = RPGTools.Device.instance.GetMemoryAppSize();
            AddMessage("async load bundle used:" + loadTime.ToString());
            AddMessage("async load bundle mem change:" + (endMem - startMem));
            if (abr != null)
            {

                AddMessage("load bunde success.");
                m_AssetBundle = abr.assetBundle;
            }
            else
            {
                AddMessage("load bunde fail.");
            }
        });
    }

    public void LoadAssetSync()
    {
        string assetPath = m_AssetPathInputField.text;
        if (string.IsNullOrEmpty(assetPath))
        {
            AddMessage("asset path is empty.");
            return;
        }

        if (m_AssetBundle != null)
        {
            long startMem = RPGTools.Device.instance.GetMemoryAppSize();
            float startTime = Time.realtimeSinceStartup;
            GameObject obj = m_AssetBundle.LoadAsset<GameObject>(assetPath);
            float loadTime = Time.realtimeSinceStartup - startTime;
            long endMem = RPGTools.Device.instance.GetMemoryAppSize();
            AddMessage("sync load asset used:" + loadTime.ToString());
            AddMessage("sync load asset mem change:" + (endMem - startMem));
            if (obj != null)
            {
                AddMessage("load asset success.");
            }
            else
            {
                AddMessage("load asset fail.");
            }
        }
    }

    public void LoadAssetASync()
    {
        string assetPath = m_AssetPathInputField.text;
        if (string.IsNullOrEmpty(assetPath))
        {
            AddMessage("asset path is empty.");
            return;
        }

        if (m_AssetBundle != null)
        {
          StartCoroutine(_LoadAssetCoroutine(assetPath));
        }
    }

    private IEnumerator _LoadAssetCoroutine(string assetPath)
    {
        long startMem = RPGTools.Device.instance.GetMemoryAppSize();
        float startTime = Time.realtimeSinceStartup;
        var request = m_AssetBundle.LoadAssetAsync<GameObject>(assetPath);
        yield return request;
        float loadTime = Time.realtimeSinceStartup - startTime;
        long endMem = RPGTools.Device.instance.GetMemoryAppSize();
        AddMessage("async load asset used:" + loadTime.ToString());
        AddMessage("async load asset mem change:" + (endMem - startMem));
        if (request.asset != null)
        {
            AddMessage("load asset success.");
        }
        else
        {
            AddMessage("load asset fail.");
        }
    }

    public void AddMessage(string message)
    {
        m_Message.text += message+"\n";
    }

    static StringBuilder _StrBuilderLine = new StringBuilder(1024);
    private static void SetPropertySize(Text uiText,  long size)
    {
        const long kb = 1024;
        const long mb = 1024 * 1024;
        //const long gb = 1024 * 1024 * 1024;

        _StrBuilderLine.Clear();
        if (size >= mb)
        {
            _StrBuilderLine.Append(Math.Round(1.0 * size / mb, 2)).Append("M");
        }
        else if (size >= kb)
        {
            _StrBuilderLine.Append(Math.Round(1.0 * size / kb, 2)).Append("K");
        }
        else
        {
            _StrBuilderLine.Append(size).Append("B");
        }

        uiText.text = _StrBuilderLine.ToString();
    }
}
