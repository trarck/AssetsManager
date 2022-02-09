using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YH.AssetManage
{
    public class InfoManager:IInfoManager
    {
        BundleManifest m_BundleManifest;
        Dictionary<string, AssetInfo> m_AssetInfos = new Dictionary<string, AssetInfo>();
        Dictionary<string, AssetBundleInfo> m_AssetBundleInfos = new Dictionary<string, AssetBundleInfo>();

        MonoBehaviour m_CoroutineExecuter;
		Coroutine m_LoadPackageFileCoroutine;

        bool m_Loading = false;
        int m_RetryTimes=AMSetting.RequestRetryTimes;

        public event Action<bool> onLoadComplete;

        public Dictionary<string,AssetBundleInfo> AssetBundleInfos
        {
            get
            {
                return m_AssetBundleInfos;
            }
        }

        public Dictionary<string,AssetInfo> AssetInfos
        {
            get
            {
                return m_AssetInfos;
            }
        }

        public InfoManager(MonoBehaviour coroutineExecuter)
        {
			m_CoroutineExecuter = coroutineExecuter;
        }

        public void Init()
        {

        }

        public void Load(string filePath)
        {
            m_Loading = true;

            if (filePath.Contains("://"))
            {
                if (!filePath.Contains(Application.version))
                {
                    filePath += "?v=" + Application.version;
                }
                LoadFromPackage(filePath);
            }
            else
            {
                LoadFromFile(filePath);
            }
        }

        public void LoadFromPackage(string url)
        {
            AMDebug.LogFormat("[AssetManage]Load File {0} ", url);
			m_LoadPackageFileCoroutine = m_CoroutineExecuter.StartCoroutine(LoadPackageFile(url));
        }

        IEnumerator LoadPackageFile(string fileUrl)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(fileUrl))
            {
                webRequest.timeout = AMSetting.DownloadTimeout;
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError && m_RetryTimes-- > 0)
                {
                        webRequest.Dispose();
                        LoadFromPackage(fileUrl);
                }
                else
                {
                    bool success = true;
                    if (string.IsNullOrEmpty(webRequest.error))
                    {
                        //save to local file
                        string bundleDir = AssetPaths.GetBundlePath();
                        if (!Directory.Exists(bundleDir))
                        {
                            Directory.CreateDirectory(bundleDir);
                        }
                        string localInfoFilePath = AssetPaths.Combine(bundleDir, AssetPaths.bundleManifestFile);
                        AMDebug.LogFormat("[AssetManage]Save all.manifest to {0} ", localInfoFilePath);
                        File.WriteAllBytes(localInfoFilePath,webRequest.downloadHandler.data);

                        using (MemoryStream stream = new MemoryStream(webRequest.downloadHandler.data))
                        {
                            LoadFromStream(stream);
                        }
                    }
                    else
                    {
                        AMDebug.LogErrorFormat("[AssetManage]LoadPackageFile:{0} error: {1} ", fileUrl, webRequest.error);
                        success = false;
                    }

                    doLoadComplete(success);
                }
            }
        }

        public void LoadFromFile(string filePath)
        {
            bool success = true;
            if (File.Exists(filePath))
            {
                using (FileStream fs = File.OpenRead(filePath))//  new FileStream(filePath, FileMode.Open))
                {
                    LoadFromStream(fs);
                }
            }
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            else
            {
                success = false;
            }
#endif

            doLoadComplete(success);
        }

        public void LoadFromStream(Stream steam)
        {
            BinaryReader reader = new BinaryReader(steam);

            if (reader.ReadChar() == 'A' && reader.ReadChar() == 'B' && reader.ReadChar() == 'M' && reader.ReadChar() == 'I')
            {
                LoadFromBinaryStream(steam);
            }
            else
            {
                LoadFromTextStream(steam);
            }
        }

        public void LoadFromTextStream(Stream steam)
        {
            steam.Position = 0;
            StreamReader reader = new StreamReader(steam);
            string content = reader.ReadToEnd();
            BundleManifest bundleManifest = JsonUtility.FromJson<BundleManifest>(content);
            AddBundleManifest(bundleManifest);
        }

        public void LoadFromBinaryStream(Stream steam)
        {
            BinaryReader reader = new BinaryReader(steam);

            //skip head sign
            //reader.ReadInt32();
            steam.Position = 4;
            BundleManifest bundleManifest = new BundleManifest();
            bundleManifest.Read(reader);

            AddBundleManifest(bundleManifest);
        }

        private void AddBundleManifest(BundleManifest bundleManifest)
        {
            if (m_BundleManifest != null)
            {
                //merge to m_BundleManifest
                m_BundleManifest.bundleInfos.AddRange(bundleManifest.bundleInfos);
            }
            else
            {
                m_BundleManifest = bundleManifest;
            }

            UpdateManifest(bundleManifest);
        }

        public void SaveBinary(string fileName)
        {
            using (FileStream fs = File.OpenWrite(fileName))// new FileStream(fileName, FileMode.Create))
            {
                fs.SetLength(0);
                SaveBinaryStream(fs);
            }
        }

        public void SaveBinaryStream(Stream stream)
        {
            if (m_BundleManifest != null)
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(0x41424D49);
                m_BundleManifest.Write(writer);
            }
        }

        protected void UpdateManifest(BundleManifest bundleManifest)
        {
            if (bundleManifest == null)
            {
                return;
            }

            //create asset bundle map
            AssetBundleInfo bundleInfo = null;
            for (int i = 0, l = bundleManifest.bundleInfos.Count; i < l; ++i)
            {
                bundleInfo = bundleManifest.bundleInfos[i];
                m_AssetBundleInfos.Add(bundleInfo.fullName, bundleInfo);
            }

            AssetInfo assetInfo = null;
            for (int i = 0, l = bundleManifest.bundleInfos.Count; i < l; ++i)
            {
                bundleInfo = bundleManifest.bundleInfos[i];

                //create asset info map
                for (int j = 0, k = bundleInfo.assets.Count; j < k; ++j)
                {
                    assetInfo = bundleInfo.assets[j];
                    assetInfo.bundleName = bundleInfo.fullName;
                    m_AssetInfos.Add(assetInfo.fullName, assetInfo);
                }
            }
        }

        public AssetInfo FindAssetInfo(string key)
        {
            if (m_AssetInfos != null && !string.IsNullOrEmpty(key))
            {
                if (m_AssetInfos.ContainsKey(key))
                {
                    return m_AssetInfos[key];
                }

                string fixKey = AssetPaths.AddAssetPrev(key);
                if (!fixKey.Equals(key))
                {
                    if (m_AssetInfos.ContainsKey(fixKey))
                    {
                        return m_AssetInfos[fixKey];
                    }
                }
            }
            return null;
        }

        public AssetInfo FindAssetInfoWithAlias(string alias)
        {
            if (m_AssetInfos != null && !string.IsNullOrEmpty(alias))
            {
                if (m_AssetInfos.ContainsKey(alias))
                {
                    return m_AssetInfos[alias];
                }
            }
            return null;
        }

        public AssetBundleInfo FindAssetBundleInfo(string key)
        {
            if (m_AssetBundleInfos != null && m_AssetBundleInfos.ContainsKey(key))
            {
                return m_AssetBundleInfos[key];
            }
            return null;
        }

        public void Clean()
        {
            onLoadComplete = null;
            m_AssetInfos.Clear();
            m_AssetBundleInfos.Clear();

			if (m_LoadPackageFileCoroutine != null && m_CoroutineExecuter != null)
			{
				m_CoroutineExecuter.StopCoroutine(m_LoadPackageFileCoroutine);
			}
        }

        protected void doLoadComplete(bool result)
        {
            AMDebug.Log("[AssetManage]Info Manager load complete");
            m_Loading = false;
            if (onLoadComplete != null)
            {
                onLoadComplete(result);
            }
        }

        public bool loading
        {
            get
            {
                return m_Loading;
            }
        }

        public string version
        {
            get
            {
                return m_BundleManifest != null ? m_BundleManifest.version : null;
            }
        }

        public int format
        {
            get
            {
                return m_BundleManifest != null ? m_BundleManifest.format : 0;
            }
        }
    }
}
