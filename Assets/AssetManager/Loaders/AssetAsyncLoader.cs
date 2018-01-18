using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetAsyncLoader : AssetLoader
    {
        protected LoaderRequest m_LoaderRequest;

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
                if (!string.IsNullOrEmpty(info.bundleName))
                {
                    assetManager.LoadAssetBundle(info.bundleName, false, (abr) =>
                    {
                        assetBundleReference = abr;
                        LoadAsset();
                    });
                }
                else
                {
                    LoadAsset();
                }
            }
            else if (isFinishedState)
            {
                DoLoadComplete();
            }
        }

        void LoadAsset()
        {
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

        void LoadScene()
        {
            //do nothing.scene just need load dependencies
            m_LoaderRequest = new EmptyLoaderRequest();
        }

        public override void Complete()
        {
            //check success or fail
            if (m_LoaderRequest != null && !m_LoaderRequest.haveError)
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

        public override void Clean()
        {
            m_LoaderRequest = null;
            base.Clean();
        }

        public override AssetReference result
        {
            get
            {
                if (state == State.Error)
                {
                    return null;
                }

                if (m_Result == null && state == State.Completed)
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
    }
}
