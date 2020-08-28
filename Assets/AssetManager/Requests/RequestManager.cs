using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManage
{
    public class RequestManager:IRequestManager
    {
        internal static readonly ObjectPool<BundleCreateRequest> BundleCreateRequestPool = new ObjectPool<BundleCreateRequest>(null, l => l.Clean());
        internal static readonly ObjectPool<BundleWebRequest> BundleWebRequestPool = new ObjectPool<BundleWebRequest>(null, l => l.Clean());
        internal static readonly ObjectPool<BundleWebSaveRequest> BundleWebSaveRequestPool = new ObjectPool<BundleWebSaveRequest>(null, l => l.Clean());

        internal static readonly ObjectPool<AssetLoaderRequest> AssetLoaderRequestPool = new ObjectPool<AssetLoaderRequest>(null, l => l.Clean());
        internal static readonly ObjectPool<ResouceLoaderRequest> ResouceLoaderRequestPool = new ObjectPool<ResouceLoaderRequest>(null, l => l.Clean());

		internal static readonly ObjectPool<EmptyLoaderRequest> EmptyLoaderRequestPool = new ObjectPool<EmptyLoaderRequest>(null, l => l.Clean());

		int m_MaxActiveRequest = 12;
        List<Request> m_ActiveRequests = new List<Request>();
        List<int> m_FinishedIndexs = new List<int>();
        Stack<Request> m_PrepareRequests = new Stack<Request>();
        

        AssetManager m_AssetManager=null;

        //cache manager
        CacheManager m_CacheManager=null;

        public RequestManager(AssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }

        public virtual void Init()
        {
#if ASSETMANAGE_BUNDLE_CACHE_ON
            m_CacheManager = new CacheManager();
            m_CacheManager.cacheInfoFile = AssetPaths.ToBundlePath(cacheManager.cacheInfoFile);
            m_CacheManager.LoadCacheInfo();
#endif
        }

        public virtual void Clean()
        {
            m_ActiveRequests.Clear();
            m_FinishedIndexs.Clear();
            m_PrepareRequests.Clear();
        }

        #region request operate
        public virtual void ActiveRequest(Request request)
        {
            if (m_ActiveRequests.Count < m_MaxActiveRequest)
            {
                m_ActiveRequests.Add(request);
                request.Start();
            }
            else
            {
                m_PrepareRequests.Push(request);
            }
        }

        public virtual void Update(float deltaTime)
        {
            //check request 
            Tick();
#if ASSETMANAGE_BUNDLE_CACHE_ON
            if (m_CacheManager != null)
            {
                m_CacheManager.Update(deltaTime);
            }
#endif
        }

        protected void Tick()
        {
            m_FinishedIndexs.Clear();

            try
            {
                Request request = null;
                for (int i = 0, l = m_ActiveRequests.Count; i < l; ++i)
                {
                    request = m_ActiveRequests[i];
                    request.Update();

                    if (request.isDone)
                    {
                        m_FinishedIndexs.Add(i);
                        request.Complete();
                        if (request.autoRelease)
                        {
                            ReleaseRequest(request);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

            CheckAndStartRequests();
        }

        protected void CheckAndStartRequests()
        {
            if (m_PrepareRequests.Count >= m_FinishedIndexs.Count)
            {
                //替换激活的请求
                Request request = null;
                for (int i = m_FinishedIndexs.Count - 1; i >= 0; --i)
                {
                    request = m_PrepareRequests.Pop();
                    m_ActiveRequests[m_FinishedIndexs[i]] = request;
                    request.Start();
                }
            }
            else
            {
                Request request = null;
                int i = 0;
                //替换一部分
                for (int l= m_PrepareRequests.Count; i<l;++i)
                {
                    request = m_PrepareRequests.Pop();
                    m_ActiveRequests[m_FinishedIndexs[i]] = request;
                    request.Start();
                }
                //移除剩余的
                for(int j = m_FinishedIndexs.Count-1; j >=i; --j)
                {
                    m_ActiveRequests.RemoveAt(m_FinishedIndexs[j]);
                }
            }
        }

        protected void ReleaseRequest(Request request)
        {
			if (request is BundleWebSaveRequest)
			{
				ReleaseBundleWebSaveRequest(request as BundleWebSaveRequest);
			}
			else if (request is BundleWebRequest)
			{
				ReleaseBundleWebRequest(request as BundleWebRequest);
			}
			else if (request is BundleCreateRequest)
			{
				ReleaseBundleCreateRequest(request as BundleCreateRequest);
			}
			else if (request is AssetLoaderRequest)
			{
				ReleaseAssetLoaderRequest(request as AssetLoaderRequest);
			}
			else if (request is ResouceLoaderRequest)
			{
				ReleaseResouceLoaderRequest(request as ResouceLoaderRequest);
			}
			else if (request is EmptyLoaderRequest)
			{
				EmptyLoaderRequestPool.Release(request as EmptyLoaderRequest);
			}
        }
		#endregion

		#region create bundle request
		public virtual Request CreateAssetBundleRequest(AssetBundleInfo assetBundleInfo)
        {
            if (assetBundleInfo == null)
            {
                return null;
            }

            if (m_CacheManager!=null)
            {
                //use cache
                if (m_CacheManager.IsCached(assetBundleInfo.fullName,assetBundleInfo.hash))
                {
                    //load from cache
                    string assetPath = AssetPaths.GetFullPath(assetBundleInfo.fullName);
                    return CreateBundleCreateRequest(assetPath);
                }
                else
                {
                    //download and save to cache
                    string url = AssetPaths.GetUrl(assetBundleInfo.fullName);
                    string savePath = AssetPaths.ToBundlePath(assetBundleInfo.fullName);
                    BundleWebSaveRequest webSaveRequest= CreateBundleWebSaveRequest(url,savePath,assetBundleInfo.hash, assetBundleInfo.fullName);
                    webSaveRequest.onSaveComplete += OnBundleWebRequestSaveComplete;
                    return webSaveRequest;
                }
            }
            else
            {
                //no cache
                string assetPath = AssetPaths.GetFullPath(assetBundleInfo.fullName);
#if ASSETMANAGER_LOG_ON
                Debug.LogFormat("[AssetManage]LoadBundle {0}---{1}", assetPath, Time.frameCount);
#endif
                if (assetPath.Contains("://"))
                {
                    return CreateBundleWebRequest(assetPath);
                }
                else
                {
                    return CreateBundleCreateRequest(assetPath);
                }
            }
        }

        private void OnBundleWebRequestSaveComplete(BundleWebSaveRequest request)
        {
            if (m_CacheManager!=null)
            {
                m_CacheManager.UpdateCacheItem(request.bundleFullname, request.hash);
            }
        }



        public static BundleWebRequest CreateBundleWebRequest(string url,string hash=null)
        {
            BundleWebRequest request = BundleWebRequestPool.Get();
            request.bundleUrl = url;
            request.hash = hash;
            request.timeout = AMSetting.DownloadTimeout;
            request.retryTimes = AMSetting.RequestRetryTimes;
            return request;
        }

        public static void ReleaseBundleWebRequest(BundleWebRequest request)
        {
            BundleWebRequestPool.Release(request);
        }

        public static BundleCreateRequest CreateBundleCreateRequest(string bundlePath)
        {
            BundleCreateRequest request = BundleCreateRequestPool.Get();
            request.bundlePath = bundlePath;
            return request;
        }

        public static void ReleaseBundleCreateRequest(BundleCreateRequest request)
        {
            BundleCreateRequestPool.Release(request);
        }

        public static BundleWebSaveRequest CreateBundleWebSaveRequest(string url,string localPath, string hash = null,string fullName=null)
        {
            BundleWebSaveRequest request = BundleWebSaveRequestPool.Get();
            request.bundleUrl = url;
            request.hash = hash;
            request.saveFilePath = localPath;
            request.bundleFullname = fullName;
            request.timeout = AMSetting.DownloadTimeout;
            request.retryTimes = AMSetting.RequestRetryTimes;
            return request;
        }

        public static void ReleaseBundleWebSaveRequest(BundleWebSaveRequest request)
        {
            BundleWebSaveRequestPool.Release(request);
        }

		#endregion

		#region create asset request

		public virtual Request CreateAssetRequest(AssetBundle assetBundle, string assetName, Type type)
		{
			return CreateAssetLoaderRequest(assetBundle, assetName, type);
		}

		public virtual Request CreateAssetRequest(string resourcePath, Type type)
		{
			return CreateResouceLoaderRequest(resourcePath, type);
		}

		public virtual Request CreateAssetRequest()
		{
			return EmptyLoaderRequestPool.Get();
		}


		public static AssetLoaderRequest CreateAssetLoaderRequest(AssetBundle assetBundle, string assetName, Type type)
        {
            AssetLoaderRequest request = AssetLoaderRequestPool.Get();
            request.assetBundle = assetBundle;
            request.assetName = assetName;
            request.type = type;
            return request;
        }

        public static void ReleaseAssetLoaderRequest(AssetLoaderRequest request)
        {
            AssetLoaderRequestPool.Release(request);
        }

        public static ResouceLoaderRequest CreateResouceLoaderRequest(string resourcePath, Type type)
        {
            ResouceLoaderRequest request = ResouceLoaderRequestPool.Get();
            request.resourcePath = resourcePath;
            request.type = type;
            return request;
        }

        public static void ReleaseResouceLoaderRequest(ResouceLoaderRequest request)
        {
            ResouceLoaderRequestPool.Release(request);
        }

        #endregion

        public void OnApplicationPause(bool pause)
        {
#if ASSETMANAGE_BUNDLE_CACHE_ON
            if (m_CacheManager != null)
            {
                m_CacheManager.SaveCacheInfo();
            }
#endif
        }

        public void OnApplicationQuit()
        {
#if ASSETMANAGE_BUNDLE_CACHE_ON
            if (m_CacheManager != null)
            {
                m_CacheManager.SaveCacheInfo();
            }
#endif
        }

        public override string ToString()
        {
            //show state
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("ActiveRequests:").Append(m_ActiveRequests.Count).Append("\n");
            sb.Append("PrepareRequests:").Append(m_PrepareRequests.Count).Append("\n");

            sb.Append("RquestPool:");
            sb.Append("BundleCreateRequestPool =(")
                .Append(BundleCreateRequestPool.countAll)
                .Append(",").Append(BundleCreateRequestPool.countActive)
                .Append(",").Append(BundleCreateRequestPool.countInactive)
                .Append("),");
            sb.Append("BundleWebRequestPool =(")
                .Append(BundleWebRequestPool.countAll)
                .Append(",").Append(BundleWebRequestPool.countActive)
                .Append(",").Append(BundleWebRequestPool.countInactive)
                .Append("),");

            sb.Append("BundleWebSaveRequestPool =(")
                .Append(BundleWebSaveRequestPool.countAll)
                .Append(",").Append(BundleWebSaveRequestPool.countActive)
                .Append(",").Append(BundleWebSaveRequestPool.countInactive)
                .Append("),");

            sb.Append("AssetLoaderRequestPool =(")
                .Append(AssetLoaderRequestPool.countAll)
                .Append(",").Append(AssetLoaderRequestPool.countActive)
                .Append(",").Append(AssetLoaderRequestPool.countInactive)
                .Append("),");

            sb.Append("ResouceLoaderRequestPool =(")
                .Append(ResouceLoaderRequestPool.countAll)
                .Append(",").Append(ResouceLoaderRequestPool.countActive)
                .Append(",").Append(ResouceLoaderRequestPool.countInactive)
                .Append("),");

            return sb.ToString();
        }

        public CacheManager cacheManager
        {
            get
            {
                return m_CacheManager;
            }
            set
            {
                m_CacheManager = value;
            }
        }
    }
}
