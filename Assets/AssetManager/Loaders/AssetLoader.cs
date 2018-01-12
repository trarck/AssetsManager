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
            if (m_State == State.Inited)
            {
                state = State.Loading;

                //正常加载Scene，不使用LoadAsset，而使用LoadAssetBundle。
                //这里加入判断防止用错。
                if (AssetIsScene())
                {
                    LoadScene();
                }
                else
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
            }
            else if (isFinishedState())
            {
                DoLoadComplete();
            }
        }

        void LoadFromAssetBundle()
        {
            if (info != null)
            {
#if SUPPORT_ASSET_ALIAS
                string assetName = info.aliasName;
#else
                string assetName = AssetPaths.AddAssetPrev(info.fullName);
#endif
                if (type == null)
                {
                    m_LoaderRequest = new AssetLoaderRequest(assetBundleReference.assetBundle.LoadAssetAsync(assetName));
                }
                else
                {
                    m_LoaderRequest = new AssetLoaderRequest(assetBundleReference.assetBundle.LoadAssetAsync(assetName, type));
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
                string resourcePath = Path.Combine(Path.GetDirectoryName(info.fullName), Path.GetFileNameWithoutExtension(info.fullName));
                resourcePath = AssetPaths.RemoveAssetPrev(resourcePath);
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

        bool AssetIsScene()
        {
            if (info != null)
            {
                return Path.GetExtension(info.fullName).Equals(".unity", System.StringComparison.CurrentCultureIgnoreCase);
            }
            return false;
        }

        void LoadScene()
        {
            //do nothing.scene just need load dependencies
            m_LoaderRequest = new EmptyLoaderRequest();
        }

        public override void Complete()
        {
            //check success or fail
            if(m_LoaderRequest!=null && !m_LoaderRequest.haveError)
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
                Debug.LogErrorFormat("Load asset {0} fail", info.fullName);
            }
            DoLoadComplete();
        }

        protected void DoLoadComplete()
        {
            //选调用onLoaded再调用onComplete,否则可能收不到Reference的onDispose事件。
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
                        m_Result = new AssetReference(m_LoaderRequest.data, info.fullName);
                        m_Result.AddTags(paramTags);
                        if (assetBundleReference != null)
                        {
                            m_Result.assetBundleReference = assetBundleReference;
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
