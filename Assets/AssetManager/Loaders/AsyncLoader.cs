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

        public string assetPaths
        {
            get { return m_AssetPath; }
            set { m_AssetPath = value; }
        }

        public Action<AssetBundle> completeHandle
        {
            get { return m_CompleteHandle; }
            set { m_CompleteHandle = value; }
        }

        public AsyncLoader()
        {

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

        protected void LoadDependencies()
        {

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

        public override AssetBundleReference GetResult()
        {
            AssetBundleReference abr = new AssetBundleReference(m_AssetBundle,0);
            abr.level = assetlevel;
            abr.AddTag(assetTag);
            return abr;
        }

        public override void Clean()
        {
            m_AssetPath = null;
            m_AssetBundle = null;
            m_CompleteHandle = null;
            m_IsDone = false;
            m_LoaderRequest = null;
            base.Clean();
        }
    }
}