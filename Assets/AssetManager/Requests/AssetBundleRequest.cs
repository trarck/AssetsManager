using UnityEngine;
using UnityEngine.Networking;

namespace YH.AssetManager
{

    public class BundleWebRequest : Request
    {
        UnityWebRequest m_Www;
        UnityWebRequestAsyncOperation m_WebRequestAsyncOperation;
        public string bundleUrl { get; set; }

        public override bool isDone
        {
            get
            {
                if (m_WebRequestAsyncOperation != null)
                {
                    return m_WebRequestAsyncOperation.isDone;
                }
                else
                {
                    return false;
                }
            }
        }

        public override AssetBundle assetBundle
        {
            get
            {
                return m_Www != null ? DownloadHandlerAssetBundle.GetContent(m_Www) : null;
            }
        }

        public BundleWebRequest()
        {

        }

        public BundleWebRequest(string url)
        {
            bundleUrl = url;            
        }

        public override void Start()
        {
            m_Www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);
            m_WebRequestAsyncOperation = m_Www.SendWebRequest();
        }

        public override bool haveError
        {
            get
            {
                if (m_Www != null)
                {
                    return m_Www.isNetworkError || m_Www.isHttpError;
                }
                return false;
            }
        }

        public override void Clean()
        {
            m_Www = null;
            m_WebRequestAsyncOperation = null;
            bundleUrl = null;
            base.Clean();
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    m_Www.Dispose();
                    m_Www = null;
                    m_WebRequestAsyncOperation = null;
                }
            }

            base.Dispose(disposing);
        }
    }

    public class BundleCreateRequest : Request
    {
        AssetBundleCreateRequest m_CreateRequest;
        
        public string bundlePath { get; set; }

        public override bool isDone
        {
            get
            {
                if (m_CreateRequest != null)
                {
                    return m_CreateRequest.isDone;
                }
                else
                {
                    return false;
                }
            }
        }

        public override AssetBundle assetBundle
        {
            get
            {
                return m_CreateRequest != null ? m_CreateRequest.assetBundle : null;
            }
        }

        public BundleCreateRequest()
        {

        }

        public BundleCreateRequest(string bundlePath)
        {
            this.bundlePath = bundlePath;
        }

        public override void Start()
        {
            m_CreateRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        }

        public override bool haveError
        {
            get
            {
                if (m_CreateRequest != null && isDone)
                {
                    return m_CreateRequest.assetBundle == null;
                }
                return false;
            }
        }

        public override void Clean()
        {
            m_CreateRequest = null;
            bundlePath = null;
            base.Clean();
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    m_CreateRequest = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
