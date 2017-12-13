using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AsyncLoader : Loader
    {
        string m_AssetPath;
        AssetBundleInfo m_Info;
        LoaderRequest m_LoaderRequest;
        bool m_IsDone=false;

        AssetBundleReference m_Result;

        public override bool isDone
        {
            get
            {
                return m_LoaderRequest != null || m_LoaderRequest.isDone;
            }
        }

        public string assetPaths
        {
            get { return m_AssetPath; }
            set { m_AssetPath = value; }
        }

        public override AssetBundleInfo info
        {
            get { return m_Info; }
            set { m_Info = value; }
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
            DoLoadComplete();
        }

        protected void DoLoadComplete()
        {
            if (onComplete != null)
            {
                onComplete(GetResult());
            }
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



        public override AssetBundleReference GetResult()
        {
            if (isDone)
            {
                if (m_Result == null)
                {
                    m_Result = new AssetBundleReference(m_LoaderRequest.assetBundle, 0);
                    m_Result.level = assetlevel;
                    m_Result.AddTag(assetTag);
                }

                return m_Result;
            }

            return null;
        }

        public override void Clean()
        {
            m_AssetPath = null;
            onComplete = null;
            m_IsDone = false;
            m_LoaderRequest = null;
            m_Result = null;
            base.Clean();
        }
    }
}