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
        AssetBundleManifest m_AssetBundleManifest;

        MonoBehaviour m_CoroutineExecuter;
		Coroutine m_LoadPackageFileCoroutine;

        bool m_Loading = false;
        int m_RetryTimes=AMSetting.RequestRetryTimes;

        public event Action<bool> onLoadComplete;

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
            AMDebug.LogFormat("[AssetManage]LoadFromPackage {0} ", url);
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

        public void LoadFromStream(Stream stream)
        {
            if (!LoadFromBinaryStream(stream))
            {
                stream.Position = 0;
                LoadFromTextStream(stream);
            }
        }

        public void LoadFromTextStream(Stream steam)
        {
            steam.Position = 0;
            StreamReader reader = new StreamReader(steam);
            string content = reader.ReadToEnd();
            AssetBundleManifest bundleManifest = JsonUtility.FromJson<AssetBundleManifest>(content);
            AddBundleManifest(bundleManifest);
        }

        public bool LoadFromBinaryStream(Stream steam)
        {
            AssetBundleManifestReader reader = new AssetBundleManifestReader(steam);
            AssetBundleManifest bundleManifest = new AssetBundleManifest();
            if(!reader.ReadManifest(ref bundleManifest))
            {
                return false;
            }

            AddBundleManifest(bundleManifest);
            return true;
        }

        private void AddBundleManifest(AssetBundleManifest bundleManifest)
        {
            m_AssetBundleManifest = bundleManifest;
            //if (m_AssetBundleManifest != null)
            //{
            //    //merge to m_BundleManifest
            //    m_AssetBundleManifest.bundleInfos.AddRange(bundleManifest.bundleInfos);
            //}
            //else
            //{
            //    m_AssetBundleManifest = bundleManifest;
            //}

            //UpdateManifest(bundleManifest);
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
            //if (m_AssetBundleManifest != null)
            //{
            //    BinaryWriter writer = new BinaryWriter(stream);
            //    writer.Write(0x41424D49);
            //    m_AssetBundleManifest.Write(writer);
            //}
        }

        protected void UpdateManifest(AssetBundleManifest bundleManifest)
        {
            //if (bundleManifest == null)
            //{
            //    return;
            //}

            ////create asset bundle map
            //AssetBundleLoadInfo bundleInfo = null;
            //for (int i = 0, l = bundleManifest.bundleInfos.Count; i < l; ++i)
            //{
            //    bundleInfo = bundleManifest.bundleInfos[i];
            //    m_AssetBundleInfos.Add(bundleInfo.fullName, bundleInfo);
            //}

            //AssetLoadInfo assetInfo = null;
            //for (int i = 0, l = bundleManifest.bundleInfos.Count; i < l; ++i)
            //{
            //    bundleInfo = bundleManifest.bundleInfos[i];

            //    //create asset info map
            //    for (int j = 0, k = bundleInfo.assets.Count; j < k; ++j)
            //    {
            //        assetInfo = bundleInfo.assets[j];
            //        assetInfo.bundleName = bundleInfo.fullName;
            //        m_AssetInfos.Add(assetInfo.path, assetInfo);
            //    }
            //}
        }

        public AssetLoadInfo GetAssetInfo(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            ulong assetPathHash = xxHash.xxHash64.ComputeHash(assetPath);
            return GetAssetInfo(assetPath, assetPathHash);
        }

        public AssetLoadInfo GetAssetInfo(string path, ulong assetPathHash)
        {
            if (m_AssetBundleManifest == null)
            {
                AMDebug.LogErrorFormat("[InfoManager]GetAssetInfo not load manifest {0}", string.IsNullOrEmpty(path) ? assetPathHash.ToString() : path);
                return null;
            }

            ulong bundleId = 0;
            AssetLoadInfo loadInfo = null;
            if (m_AssetBundleManifest._AssetIdToBundleIds.TryGetValue(assetPathHash, out bundleId))
            {
                loadInfo = LoadInfoPool.GetAssetLoadInfo();
                loadInfo.pathHash = assetPathHash;
                loadInfo.bunldeId = bundleId;
                if (!string.IsNullOrEmpty(path))
                {
                    loadInfo.path = path;
                }
            }
            return loadInfo;
        }

        public AssetLoadInfo GetAssetInfoWithAlias(string alias)
        {
            return null;
        }

        public AssetBundleLoadInfo GetAssetBundleInfo(ulong bundleId)
        {
            if (m_AssetBundleManifest == null)
            {
                AMDebug.LogErrorFormat("[InfoManager]GetAssetBundleInfo not load manifest {0}", bundleId);
                return null;
            }

            AssetBundleRuntimeInfo assetBundleInfo = null;
            if(!m_AssetBundleManifest._Bundles.TryGetValue(bundleId,out assetBundleInfo) )
            {
                return null;
            }

            AssetBundleLoadInfo loadInfo = LoadInfoPool.GetAssetBundleLoadInfo();
            loadInfo.bundleId = bundleId;
            loadInfo.assetBundleInfo = assetBundleInfo;
            return loadInfo;
        }

        public void Clean()
        {
            onLoadComplete = null;
            m_AssetBundleManifest = null;

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

        public Version version
        {
            get
            {
                return m_AssetBundleManifest != null ? m_AssetBundleManifest.version : null;
            }
        }

        public byte format
        {
            get
            {
                return m_AssetBundleManifest != null ? m_AssetBundleManifest.format : (byte)0;
            }
        }

        public bool useBundleDependenciesAll
        {
            get
            {
                return m_AssetBundleManifest != null ? m_AssetBundleManifest.bundleDependenciesAll : false;
            }
        }
    }
}
