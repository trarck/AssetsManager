using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YH.AssetManage
{
    public class AcceptAllCertificatesSignedHandler : CertificateHandler
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
        public int timeout = 12;
        public int retryTimes = 3;

        protected bool m_Aborted = false;

        public override bool isDone
        {
            get
            {
                if (m_Aborted)
                {
                    return true;
                }

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

        protected virtual void SendRequest()
        {
            AMDebug.LogFormat("[AssetManage]BundleWebRequest Get url:{0},hash:{1}", bundleUrl,hash);
            //通过UnityWebRequest从远程下载的AssetBunle的缓存只与hash值有关与url地址无关。
            if (string.IsNullOrEmpty(hash))
            {
                m_Www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);
            }
            else
            {
                m_Www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl, Hash128.Parse(hash));
            }
            m_Www.timeout = timeout;

#if SSH_ACCEPT_ALL
            m_Www.certificateHandler = new AcceptAllCertificatesSignedHandler();
#endif
            m_WebRequestAsyncOperation = m_Www.SendWebRequest();
        }

        public override void Start()
        {
            SendRequest();
        }

        protected void Retry()
        {
            if (m_Www!=null)
            {
                m_Www.Dispose();
            }

            SendRequest();
        }

        public override void Update()
        {
            base.Update();
            if (m_Www != null && m_Www.isNetworkError)
            {
                AMDebug.LogFormat("[AssetManage]The WebRequest have network error:{0} .Left retry times:{1}",m_Www.error, retryTimes);
                //when network error retry again
                if (retryTimes > 0)
                {
                    --retryTimes;
                    Retry();
                }
            }
        }

        public override void Abort()
        {
            if (!isDone)
            {
                m_Aborted = true;

                m_WebRequestAsyncOperation = null;

                if (m_Www != null)
                {
                    m_Www.Dispose();
                }
                m_Www = null;
            }
        }

        public override bool haveError
        {
            get
            {
                return haveWWWError;
            }
        }

        public override void Complete()
        {
            if (!m_Aborted)
            {
                //clear other cached version
                string bundleName = Path.GetFileName(bundleUrl);
                Caching.ClearOtherCachedVersions(bundleName, Hash128.Parse(hash));

                base.Complete();
            }
        }

        public override void Clean()
        {
            m_WebRequestAsyncOperation = null;

            if (m_Www != null)
            {
                m_Www.Dispose();
            }
            m_Www = null;
            bundleUrl = null;
            hash = null;
            timeout = 0;
            retryTimes = 0;
            m_Aborted = false;
            base.Clean();
        }

        public override string ToString()
        {
            return string.Format("BundleWebRequest:{0},isDone:{1},progress:{2}",  bundleUrl, isDone,m_Www!=null ?m_Www.downloadProgress:-1 );
        }

		protected bool haveWWWError
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
    }

    public class BundleWebSaveRequest : BundleWebRequest
    {
        public string saveFilePath { get; set; }
        public string bundleFullname { get; set; }
        public event Action<BundleWebSaveRequest> onSaveComplete;

        AssetBundleCreateRequest m_CreateRequest;
        bool m_SaveComplete = false;        

        public override bool isDone
        {
            get
            {
				if (m_Aborted || haveWWWError)
				{
					return true;
				}

				if (m_CreateRequest != null)
                {
                    return m_CreateRequest.isDone && m_SaveComplete;
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

        protected override void SendRequest()
        {
            AMDebug.LogFormat("[AssetManage]BundleWebRequest Get url:{0},hash:{1}", bundleUrl, hash);
            m_Www = UnityWebRequest.Get(bundleUrl);
            m_Www.timeout = timeout;

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
                AMDebug.LogFormat("[AssetManage]BundleWebSaveRequest Download Complete {0}", bundleUrl);
				if (!haveWWWError)
				{
					m_CreateRequest = AssetBundle.LoadFromMemoryAsync(m_WebRequestAsyncOperation.webRequest.downloadHandler.data);
					//save to cache
#if ASSET_MANAGE_SAVE_CACHE_SYNC
            SaveSync(saveFilePath, m_Www.downloadHandler.data);
#else
					//Async save.
					SaveAsync(saveFilePath, m_Www.downloadHandler.data);
#endif
				}
				else
				{
					AMDebug.LogFormat("[AssetManage]BundleWebSaveRequest Download fail {0}", m_Www.error);
				}
			}
        }

		public override bool haveError
		{
			get
			{
				if (haveWWWError)
				{
					return true;
				}

				if (m_CreateRequest != null && isDone)
				{
					return m_CreateRequest.assetBundle == null;
				}
				return false;
			}
		}

		public override void Clean()
        {
            saveFilePath = null;
            m_CreateRequest = null;
            m_SaveComplete = false;
            onSaveComplete = null;
            base.Clean();
        }

#if ASSET_MANAGE_SAVE_CACHE_SYNC
        void SaveSync(string saveFile,byte[] data)
        {
            string saveDir = Path.GetDirectoryName(saveFile);
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            File.WriteAllBytes(saveFile, data);
            onSaveComplete?.Invoke(this);
            m_SaveComplete=true;
        }
#else
        protected async void SaveAsync(string saveFile, byte[] data)
        {
            string saveDir = Path.GetDirectoryName(saveFile);
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            using (FileStream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None,
                                                            bufferSize: 4096, useAsync: true))
            {
                await stream.WriteAsync(data, 0, data.Length);
                onSaveComplete?.Invoke(this);
                m_SaveComplete = true;
            };
        }
#endif

        public override AssetBundle assetBundle
        {
            get
            {
                return m_CreateRequest != null ? m_CreateRequest.assetBundle : null;
            }
        }
    }

    public class BundleCreateAsyncRequest : Request
    {
        AssetBundleCreateRequest m_CreateRequest;
        
        public string path { get; set; }

        public uint offset { get; set; }

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

        public BundleCreateAsyncRequest()
        {

        }

        public BundleCreateAsyncRequest(string path, uint offset)
        {
            this.path = path;
            this.offset = offset;
        }

        public override void Start()
        {
            m_CreateRequest = AssetBundle.LoadFromFileAsync(path, 0, offset);
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
            path = null;
            base.Clean();
        }

        public override string ToString()
        {
            return string.Format("BundleCreateRequest:{0},isDone:{1}", path, isDone);
        }
    }

    public class BundleCreateSyncRequest : Request
    {
        public string path { get; set; }
        public uint offset { get; set; }

        private AssetBundle m_AssetBundle = null;

        public override bool isDone
        {
            get
            {
                return true;
            }
        }

        public override float progress
        {
            get
            {
                return 1;
            }
        }

        public override AssetBundle assetBundle
        {
            get
            {
                return m_AssetBundle;
            }
        }

        public BundleCreateSyncRequest()
        {

        }

        public BundleCreateSyncRequest(string path, uint offset)
        {
            this.path = path;
            this.offset = offset;
        }

        public override void Start()
        {
            m_AssetBundle = AssetBundle.LoadFromFile(path, 0, offset);
        }

        public override bool haveError
        {
            get
            {
                return m_AssetBundle==null;
            }
        }

        public override void Clean()
        {
            m_AssetBundle = null;
            path = null;
            base.Clean();
        }

        public override string ToString()
        {
            return string.Format("BundleCreateRequest:{0},isDone:{1}", path, isDone);
        }
    }
}
