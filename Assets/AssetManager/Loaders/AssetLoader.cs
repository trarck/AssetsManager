using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetLoader : Loader
    {
        LoaderRequest m_LoaderRequest;

        AssetReference m_Result;

        public Action<AssetReference> onComplete;

        public Action<AssetLoader> onLoaded;

        public AssetInfo info { get; set; }

        public Type type{get; set;}

        public AssetBundleReference assetBundleReference { get; set; }

        public override bool isDone
        {
            get
            {
                return forceDone || m_LoaderRequest != null && m_LoaderRequest.isDone;
            }
        }

        public override void Start()
        {
            if (assetBundleReference != null)
            {
                LoadFromAssetBundle();
            }
            else
            {
                LoadFromResources();
            }
        }

        void LoadFromAssetBundle()
        {
            if (info != null)
            {
                if (type == null)
                {
                    m_LoaderRequest = new AssetLoaderRequest(assetBundleReference.assetBundle.LoadAssetAsync(info.name));
                }
                else
                {
                    m_LoaderRequest = new AssetLoaderRequest(assetBundleReference.assetBundle.LoadAssetAsync(info.name, type));
                }                
            }
            else
            {
                Error();
            }
        }

        void LoadFromResources()
        {
            if (info != null)
            {
                if (type == null)
                {
                    m_LoaderRequest = new ResouceLoaderRequest(Resources.LoadAsync(info.name));
                }
                else
                {
                    m_LoaderRequest = new ResouceLoaderRequest(Resources.LoadAsync(info.name, type));
                }                
            }
            else
            {
                Error();
            }
        }

        public override void Complete()
        {
            DoLoadComplete();
        }

        public override void Error()
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
                onComplete(this.result);
            }
        }

        public override void Clean()
        {
            onComplete = null;
            m_LoaderRequest = null;
            m_Result = null;
            base.Clean();
        }

        public AssetReference result
        {
            get
            {
                if (isDone)
                {
                    if (m_Result == null)
                    {
                        m_Result = new AssetReference(m_LoaderRequest.data, info.name);
                        m_Result.level = paramLevel;
                        if (!string.IsNullOrEmpty(paramTag))
                        {
                            m_Result.AddTag(paramTag);
                        }
                        if (assetBundleReference != null)
                        {
                            m_Result.assetBundleReference = assetBundleReference;
                        }
                    }

                    return m_Result;
                }

                return null;
            }

            set
            {
                m_Result = value;
            }
           
        }

        public void SetResult(AssetReference ar)
        {
            m_Result = ar;
        }
    }
}
