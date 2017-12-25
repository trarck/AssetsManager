using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetBundleLoader : Loader
    {
        LoaderRequest m_LoaderRequest;

        AssetBundleReference m_Result;

        public Action<AssetBundleReference> onComplete;

        public Action<AssetBundleLoader> onLoaded;

        public override bool isDone
        {
            get
            {
                return forceDone || m_LoaderRequest != null && m_LoaderRequest.isDone;
            }
        }

        public AssetBundleInfo info { get; set; }

        public override void Start()
        {
            if (m_State == State.Idle)
            {
                if (info != null)
                {
                    state = State.Loading;

                    string assetPath = AssetPaths.GetFullPath(info.fullName);
                    if (assetPath.Contains("://"))
                    {
                        this.LoadFromPackage(assetPath);
                    }
                    else
                    {
                        this.LoadFromFile(assetPath);
                    }
                }
                else
                {
                    Error();
                    Debug.LogError("AssetBundleLoader can't start without info");
                }
            }
            else if (m_State != State.Loading)
            {
                DoLoadComplete();
            }
        }

        public override void Complete()
        {
            if (m_LoaderRequest != null && m_LoaderRequest.assetBundle != null)
            {
                state = State.Completed;
                DoLoadComplete();
            }
            else
            {
                Error();
            }
        }

        public override void Error()
        {
            state = State.Error;
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
                onComplete(this.result);
            }
        }

        protected void LoadDependencies()
        {

        }

        protected LoaderRequest LoadFromFile(string path)
        {
            m_LoaderRequest =new BundleLoaderRequest(AssetBundle.LoadFromFileAsync(path));
            return m_LoaderRequest;
        }

        protected LoaderRequest LoadFromPackage(string path)
        {
            m_LoaderRequest = new WWWRequest(new WWW(path));
            return m_LoaderRequest;
        }

        public AssetBundleReference result
        {
            get
            {
                if (m_Result == null)
                {
                    if (isDone)
                    {

                        m_Result = new AssetBundleReference(m_LoaderRequest.assetBundle, info.fullName);
                        m_Result.level = paramLevel;
                        if (!string.IsNullOrEmpty(paramTag))
                        {
                            m_Result.AddTag(paramTag);
                        }

                    }
                }
                return m_Result;
            }
            set
            {
                m_Result = value;
            }
        }

        public override void Clean()
        {
            onComplete = null;
            m_LoaderRequest = null;
            m_Result = null;
            base.Clean();
        }
    }
}