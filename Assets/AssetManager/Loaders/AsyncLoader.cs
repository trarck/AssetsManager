using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AsyncLoader : ILoader
    {
        string m_AssetPath;
        Action<AssetBundle> m_CompleteHandle;
        AssetBundle m_AssetBundle;
        AssetBundleCreateRequest m_Request;

        public AsyncLoader(string assetPath, Action<AssetBundle> completeHandle)
        {
            m_AssetPath = assetPath;
            m_CompleteHandle = completeHandle;
        }

        public void Start()
        {
            
        }

        public void Update()
        {
            if (m_Request.isDone)
            {
                //load back
                m_AssetBundle = m_Request.assetBundle;
                DoLoadComplete();
            }
        }

        protected void LoadFromFile(string path)
        {
            m_Request = AssetBundle.LoadFromFileAsync(path);
        }

        protected IEnumerator LoadFromFileYield(string path)
        {
            AssetBundleCreateRequest request =AssetBundle.LoadFromFileAsync(path);
            yield return request;
            //load back
            m_AssetBundle = request.assetBundle;
            DoLoadComplete();            
        }

        protected void LoadFromPackage(string path)
        {

        }

        protected void DoLoadComplete()
        {
            if (m_CompleteHandle != null)
            {
                m_CompleteHandle(m_AssetBundle);
            }
        }
    }
}