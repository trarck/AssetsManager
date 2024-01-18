using System.IO;

namespace YH.AssetManage
{
    public class AssetAsyncLoader : AssetLoader
    {
        AssetBundleLoader m_AssetBundleLoader = null;
        Request m_Request = null;

        public override bool isDone
        {
            get
            {
                return forceDone  || state==State.Completed || state == State.Error;
            }
        }

        public override void Start()
        {
            if (m_State == State.Inited)
            {
                state = State.Loading;
                if (info.bunldeId!=0)
                {
                    // load from bundle
					m_AssetBundleLoader = loaderManager.LoadAssetBundleAsync(info.bunldeId, 0, AMSetting.CacheDependencyBundle, OnAssetBundleLoadComplete);
                }
                else
                {
                    // load from resource
                    LoadAsset();
                }
            }
            else if (isFinishedState)
            {
                DoLoadComplete();
            }
        }

        public override void Abort()
        {
            //如果已经完成或出错，则无效。
            if(m_State==State.Error || m_State == State.Completed)
            {
                return;
            }

            base.Abort();
            //取消正在加载的资源回调。
            if (m_AssetBundleLoader != null)
            {
                m_AssetBundleLoader.onComplete -= OnAssetBundleLoadComplete;
                m_AssetBundleLoader.DecreaseLoadingCount();
                m_AssetBundleLoader = null;
            }

            if (m_Request != null)
            {
                m_Request.onComplete -= OnRequestComplete;
                m_Request.Abort();
                m_Request = null;
            }

            //触发一次完成事件。以便管理器清除load数据。
            DoLoadComplete();
        }

        void OnAssetBundleLoadComplete(AssetBundleReference abr)
        {
            //置空loader
            if (m_AssetBundleLoader != null)
            {
                m_AssetBundleLoader.onComplete -= OnAssetBundleLoadComplete;
                m_AssetBundleLoader = null;
            }

            //为了保险做一下检查
            if (!m_Aborted)
            {
                assetBundleReference = abr;
                m_State = State.Loaded;
                LoadAsset();
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
                string assetName = info.path;
                if (string.IsNullOrEmpty(assetName))
                {
                    //TODO use FixeStringPool
                    assetName = HexConverter.ToString(info.pathHash, HexConverter.Casing.Lower);
                }

                AMDebug.LogFormat("[AssetManage]Load asset {0}", assetName);

                m_Request = loaderManager.requestManager.CreateAssetRequest(assetBundleReference.assetBundle, assetName, type);
                m_Request.onComplete += OnRequestComplete;
				loaderManager.requestManager.ActiveRequest(m_Request);
            }
            else
            {
                AMDebug.LogError("[AssetManage]Load Asset with no info");
                Error();
            }
        }

        void LoadFromResources()
        {
            if (info != null)
            {
                string resourcePath = Path.Combine(Path.GetDirectoryName(info.path), Path.GetFileNameWithoutExtension(info.path));
                resourcePath = AssetPaths.RemoveAssetPrev(resourcePath);
                m_Request = loaderManager.requestManager.CreateAssetRequest(resourcePath, type);
                m_Request.onComplete += OnRequestComplete;
				loaderManager.requestManager.ActiveRequest(m_Request);
            }
            else
            {
                Error();
                AMDebug.LogError("[AssetManage]Load Asset with no info");
            }
        }

        void LoadScene()
        {
            //do nothing.scene just need load dependencies
            m_Request = loaderManager.requestManager.CreateAssetRequest();
            m_Request.onComplete += OnRequestComplete;
			loaderManager.requestManager.ActiveRequest(m_Request);
        }

        protected void OnRequestComplete(Request request)
        {
            if (m_Request != null)
            {
                m_Request.onComplete -= OnRequestComplete;
                m_Request = null;
            }

            if (!request.haveError)
            {
                result = new AssetReference(request.asset, info.pathHash);
                m_Result.AddTags(paramTags);
                if (assetBundleReference != null)
                {
                    result.assetBundleReference = assetBundleReference;
                }

                Complete();
            }
            else
            {
                Error();
            }
        }

        public override void Clean()
        {
            base.Clean();
            m_AssetBundleLoader = null;
            m_Request = null;
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
                //SetResult(value);
                base.result = value;
            }
        }
    }
}
