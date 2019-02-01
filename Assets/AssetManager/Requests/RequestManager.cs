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

        internal static readonly ObjectPool<AssetLoaderRequest> AssetLoaderRequestPool = new ObjectPool<AssetLoaderRequest>(null, l => l.Clean());
        internal static readonly ObjectPool<ResouceLoaderRequest> ResouceLoaderRequestPool = new ObjectPool<ResouceLoaderRequest>(null, l => l.Clean());

        int m_MaxActiveRequest = 5;
        List<Request> m_ActiveRequests = ListPool<Request>.Get();
        List<int> m_FinishedIndexs = ListPool<int>.Get();
        Stack<Request> m_PrepareRequests = StackPool<Request>.Get();
        

        AssetManager m_AssetManager;

        public RequestManager(AssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }

        public void Clean()
        {
            ListPool<Request>.Release(m_ActiveRequests);
            ListPool<int>.Release(m_FinishedIndexs);
            StackPool<Request>.Release(m_PrepareRequests);
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
                            request.Dispose();
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
        #endregion

        #region create bundle request

        public static BundleWebRequest CreateBundleWebRequest(string url)
        {
            BundleWebRequest request = BundleWebRequestPool.Get();
            request.bundleUrl = url;
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
    }
}
