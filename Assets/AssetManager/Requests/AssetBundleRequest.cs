using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YH.AssetManager
{
    class AcceptAllCertificatesSignedHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public class BundleWebRequest : Request
    {
        protected UnityWebRequest m_Www;
        protected UnityWebRequestAsyncOperation m_WebRequestAsyncOperation;
        public string bundleUrl { get; set; }
        public string hash { get; set; }

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

        public override float progress
        {
            get
            {
                if (m_WebRequestAsyncOperation != null)
                {
                    return m_WebRequestAsyncOperation.progress;
                }
                else
                {
                    return 0;
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
            //通过UnityWebRequest从远程下载的AssetBunle的缓存只与hash值有关与url地址不关。
            if (string.IsNullOrEmpty(hash))
            {
                m_Www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);
            }
            else
            {
                m_Www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl,Hash128.Parse(hash));
            }

#if SSH_ACCEPT_ALL
            m_Www.certificateHandler = new AcceptAllCertificatesSignedHandler();
#endif

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
            if (m_Www != null)
            {
                m_Www.Dispose();
            }
            m_Www = null;
            m_WebRequestAsyncOperation = null;
            bundleUrl = null;
            hash = null;
            base.Clean();
        }

        public override string ToString()
        {
            return string.Format("BundleWebRequest:{0},isDone:{1}",  bundleUrl, isDone);
        }
    }

    public class BundleWebSaveRequest : BundleWebRequest
    {
        public string saveFilePath { get; set; }

        AssetBundleCreateRequest m_CreateRequest;

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

        public override float progress
        {
            get
            {
                if (m_WebRequestAsyncOperation != null)
                {
                    return m_WebRequestAsyncOperation.progress*0.5f;
                }
                else if (m_CreateRequest != null)
                {
                    return m_CreateRequest.progress*0.5f+0.5f;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override void Start()
        {
            if (string.IsNullOrEmpty(hash))
            {
                m_Www = UnityWebRequest.Get(bundleUrl);
            }
            else
            {
                m_Www = UnityWebRequest.Get(bundleUrl+"?hash="+hash);
            }

            //if (string.IsNullOrEmpty(hash))
            //{
            //    m_Www =  UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);
            //}
            //else
            //{
            //    m_Www =UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl, Hash128.Parse(hash));
            //}

#if SSH_ACCEPT_ALL
            m_Www.certificateHandler = new AcceptAllCertificatesSignedHandler();
#endif
            m_WebRequestAsyncOperation = m_Www.SendWebRequest();
        }

        public override void Update()
        {
            base.Update();
            if(m_WebRequestAsyncOperation!=null && m_WebRequestAsyncOperation.isDone && m_CreateRequest==null)
            {
#if ASSETMANAGER_LOG
                Debug.LogFormat("BundleWebSaveRequest Download Complete {0},{1}", bundleUrl, Time.frameCount);
#endif
                m_CreateRequest = AssetBundle.LoadFromMemoryAsync(m_WebRequestAsyncOperation.webRequest.downloadHandler.data);
            }
        }

        public override void Clean()
        {
            base.Clean();
            saveFilePath = null;
            m_CreateRequest = null;
        }

        public override void Complete()
        {
            base.Complete();
            //Async save.如果Unity不支持则改为同步版本。
            SaveAsync(saveFilePath, m_Www.downloadHandler.data);
        }

        protected async void SaveAsync(string saveFile, byte[] data)
        {
            using (FileStream stream = new FileStream(saveFile, FileMode.Truncate, FileAccess.Write, FileShare.None,
                                                            bufferSize: 4096, useAsync: true))
            {
                await stream.WriteAsync(data, 0, data.Length);
            };
        }

        public override AssetBundle assetBundle
        {
            get
            {
                return m_CreateRequest != null ? m_CreateRequest.assetBundle : null;
            }
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

        public override float progress
        {
            get
            {
                if (m_CreateRequest != null)
                {
                    return m_CreateRequest.progress;
                }
                else
                {
                    return 0;
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

        public override string ToString()
        {
            return string.Format("BundleCreateRequest:{0},isDone:{1}", bundlePath, isDone);
        }
    }
}
