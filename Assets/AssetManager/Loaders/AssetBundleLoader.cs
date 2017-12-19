using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetBundleLoader : Loader
    {
        AssetBundleInfo m_Info;
        LoaderRequest m_LoaderRequest;
        bool m_IsDone=false;

        AssetBundleReference m_Result;

        public Action<AssetBundleReference> onComplete;

        public Action<AssetBundleLoader> onLoaded;

        public int assetBundlelevel { get; set; }
        public string assetBundleTag { get; set; }

        public override bool isDone
        {
            get
            {
                return m_LoaderRequest != null || m_LoaderRequest.isDone;
            }
        }

        public AssetBundleInfo info
        {
            get { return m_Info; }
            set { m_Info = value; }
        }

        public override void Start()
        {
            string assetPath = AssetPaths.GetFullPath(m_Info.fullName);
            if (assetPath.Contains("://"))
            {
                this.LoadFromPackage(assetPath);
            }
            else
            {
                this.LoadFromFile(assetPath);
            }
        }

        public override void Complete()
        {
            DoLoadComplete();
        }

        protected void DoLoadComplete()
        {
            if (onLoaded != null)
            {
                onLoaded(this);
            }

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
                    m_Result = new AssetBundleReference(m_LoaderRequest.assetBundle, assetBundlelevel);
                    m_Result.assetBundleName = m_Info.fullName;
                    m_Result.AddTag(assetBundleTag);
                }

                return m_Result;
            }

            return null;
        }

        public override void Clean()
        {
            onComplete = null;
            m_IsDone = false;
            m_LoaderRequest = null;
            m_Result = null;
            base.Clean();
        }
    }
}