using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class RequestManager
    {
        internal static readonly ObjectPool<BundleCreateRequest> BundleCreateRequestPool = new ObjectPool<BundleCreateRequest>(null, l => l.Clean());
        internal static readonly ObjectPool<BundleWebRequest> BundleWebRequestPool = new ObjectPool<BundleWebRequest>(null, l => l.Clean());
        internal static readonly ObjectPool<BundleWebSaveRequest> BundleWebSaveRequestPool = new ObjectPool<BundleWebSaveRequest>(null, l => l.Clean());

        internal static readonly ObjectPool<AssetLoaderRequest> AssetLoaderRequestPool = new ObjectPool<AssetLoaderRequest>(null, l => l.Clean());
        internal static readonly ObjectPool<ResouceLoaderRequest> ResouceLoaderRequestPool = new ObjectPool<ResouceLoaderRequest>(null, l => l.Clean());

        int m_MaxActiveRequest = 12;
        List<Request> m_ActiveRequests = new List<Request>();
        List<int> m_FinishedIndexs = new List<int>();
        Stack<Request> m_PrepareRequests = new Stack<Request>();
        

        AssetManager m_AssetManager;

        public RequestManager(AssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }

        public void Clean()
        {
            m_ActiveRequests.Clear();
            m_FinishedIndexs.Clear();
            m_PrepareRequests.Clear();
        }

        #region request operate
        public void ActiveRequest(Request request)
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

        public void Update()
        {
            //check request 
            Tick();
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
            else if(request is BundleWebRequest)
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
        }
        #endregion

        #region create bundle request

        public static BundleWebRequest CreateBundleWebRequest(string url,string hash=null)
        {
            BundleWebRequest request = BundleWebRequestPool.Get();
            request.bundleUrl = url;
            request.hash = hash;
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

        public static BundleWebSaveRequest CreateBundleWebSaveRequest(string url,string localPath, string hash = null)
        {
            BundleWebSaveRequest request = BundleWebSaveRequestPool.Get();
            request.bundleUrl = url;
            request.hash = hash;
            request.saveFilePath = localPath;
            return request;
        }

        public static void ReleaseBundleWebSaveRequest(BundleWebSaveRequest request)
        {
            BundleWebSaveRequestPool.Release(request);
        }

        #endregion

        #region create asset request

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
    }
}
