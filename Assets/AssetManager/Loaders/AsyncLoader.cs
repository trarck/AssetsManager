using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AsyncLoader : Loader
    {
        string m_AssetPath;
        Action<AssetBundle> m_CompleteHandle;
        AssetBundle m_AssetBundle;
        LoaderRequest m_LoaderRequest;

        bool m_IsDone=false;

        public override bool isDone
        {
            get
            {
                return m_LoaderRequest != null || m_LoaderRequest.isDone;
                //return m_IsDone;  
            }
        }

        public AsyncLoader(string assetPath, Action<AssetBundle> completeHandle)
        {
            m_AssetPath = assetPath;
            m_CompleteHandle = completeHandle;
        }

        public override void Start()
        {
            if (m_AssetPath.Contains("://"))
            {
                this.LoadFromPackage(m_AssetPath);
            }
            else
            {
                this.LoadFromPackage(m_AssetPath);
            }
        }

        public override void Complete()
        {
            if (m_LoaderRequest!=null)
            {
                m_AssetBundle = m_LoaderRequest.assetBundle;
            }
            DoLoadComplete();
        }

        protected LoaderRequest LoadFromFile(string path)
        {
            m_LoaderRequest =new AsyncRequest(AssetBundle.LoadFromFileAsync(path));
            return m_LoaderRequest;
        }

        protected LoaderRequest LoadFromPackage(string path)
        {
            m_LoaderRequest = new WWWRequest(new WWW(path));
            return m_LoaderRequest;
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