using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetLoader : Loader
    {
        protected LoaderRequest m_LoaderRequest;

        protected AssetReference m_Result;

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
            if (m_State == State.Idle)
            {
                state = State.Loading;

                if (assetBundleReference != null)
                {
                    LoadFromAssetBundle();
                }
                else
                {
                    LoadFromResources();
                }
            }
            else if (m_State != State.Loading)
            {
                DoLoadComplete();
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
                Debug.LogError("Load Asset with no info");
            }
        }

        void LoadFromResources()
        {
            if (info != null)
            {
                string resourcePath = Path.Combine(Path.GetDirectoryName(info.name), Path.GetFileNameWithoutExtension(info.name));

                if (type == null)
                {
                    m_LoaderRequest = new ResouceLoaderRequest(Resources.LoadAsync(resourcePath));
                }
                else
                {
                    m_LoaderRequest = new ResouceLoaderRequest(Resources.LoadAsync(resourcePath, type));
                }                
            }
            else
            {
                Error();
                Debug.LogError("Load Asset with no info");
            }
        }

        public override void Complete()
        {
            //check success or fail
            if(m_LoaderRequest!=null && m_LoaderRequest.data != null)
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
            if (info != null)
            {
                Debug.LogErrorFormat("Load asset {0} fail", info.name);
            }
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
                if (m_Result == null && state==State.Completed)
                {
                    if (isDone)
                    {
                        if (m_LoaderRequest.data != null)
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
                    }
                }
                return m_Result;
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
