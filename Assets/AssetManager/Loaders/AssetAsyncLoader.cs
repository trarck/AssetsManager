using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetAsyncLoader : AssetLoader
    {
        public override bool isDone
        {
            get
            {
                return forceDone || state==State.Loaded || state==State.Completed;
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

        public override void Update()
        {
            switch (m_State)
            {
                case State.Loading:
                    if (isDone)
                    {
                        //Complete();
                    }
                    break;
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
#if ASSETMANAGER_LOG
                Debug.LogFormat("Load asset {0}", assetName);
#endif

                Request request=RequestManager.CreateAssetLoaderRequest(assetBundleReference.assetBundle, assetName, type);
                request.onComplete += OnRequestComplete;
                assetManager.requestManager.ActiveRequest(request);
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
                Request request = RequestManager.CreateResouceLoaderRequest(resourcePath, type);
                request.onComplete += OnRequestComplete;
                assetManager.requestManager.ActiveRequest(request);
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
            Request request = new EmptyLoaderRequest();
            request.onComplete += OnRequestComplete;
            assetManager.requestManager.ActiveRequest(request);
        }

        protected void OnRequestComplete(Request request)
        {
            if (!request.haveError)
            {
                state = State.Completed;

                m_Result = new AssetReference(request.data, info.fullName);
                m_Result.AddTags(paramTags);
                if (assetBundleReference != null)
                {
                    m_Result.assetBundleReference = assetBundleReference;
                }

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

        public override AssetReference result
        {
            get
            {
                if (state == State.Error)
                {
                    return null;
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
