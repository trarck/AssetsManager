﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YH.AssetManager
{
    public class InfoManager
    {
        BundleManifest m_BundleManifest;
        Dictionary<string, AssetInfo> m_AssetInfos = new Dictionary<string, AssetInfo>();
        Dictionary<string, AssetBundleInfo> m_AssetBundleInfos = new Dictionary<string, AssetBundleInfo>();

        AssetManager m_AssetManager;

        bool m_Inited = false;

        public Action<bool> onInitComplete;

        public InfoManager(AssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }

        public void Load(string filePath)
        {
            if (filePath.Contains("://"))
            {
                LoadFromPackage(filePath);
            }
            else
            {
                LoadFromFile(filePath);
            }
        }

        public void LoadFromPackage(string filePath)
        {
            m_AssetManager.StartCoroutine(LoadPackageFile(filePath));
        }

        IEnumerator LoadPackageFile(string fileUrl)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(fileUrl))
            {
                yield return webRequest;
                bool success = true;
                if (webRequest.error == null)
                {
                    using (MemoryStream stream = new MemoryStream(webRequest.downloadHandler.data))
                    {
                        LoadFromStream(stream);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("LoadPackageFile:{0} error: {1} ", fileUrl, webRequest.error);
                    success = false;
                }

                InitComplete(success);
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

            InitComplete(success);
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

            UpdateManifest();
        }

        public void LoadFromTextStream(Stream steam)
        {
            steam.Position = 0;
            StreamReader reader = new StreamReader(steam);
            string content = reader.ReadToEnd();
            m_BundleManifest = JsonUtility.FromJson<BundleManifest>(content);
        }

        public void LoadFromBinaryStream(Stream steam)
        {
            BinaryReader reader = new BinaryReader(steam);

            //skip head sign
            //reader.ReadInt32();
            steam.Position = 4;
            m_BundleManifest = new BundleManifest();
            m_BundleManifest.Read(reader);
        }

        public void SaveToBinary(string fileName)
        {
            using (FileStream fs = File.OpenWrite(fileName))// new FileStream(fileName, FileMode.Create))
            {
                fs.SetLength(0);
                SaveToStream(fs);
            }
        }

        public void SaveToStream(Stream stream)
        {
            if (m_BundleManifest != null)
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(0x41424D49);
                m_BundleManifest.Write(writer);
            }
        }

        void UpdateManifest()
        {
            if (m_BundleManifest == null)
            {
                return;
            }

            //create asset bundle map
            AssetBundleInfo bundleInfo = null;
            for (int i = 0, l = m_BundleManifest.bundleInfos.Count; i < l; ++i)
            {
                bundleInfo = m_BundleManifest.bundleInfos[i];
                m_AssetBundleInfos.Add(bundleInfo.fullName, bundleInfo);
            }

            AssetInfo assetInfo = null;
            for (int i = 0, l = m_BundleManifest.bundleInfos.Count; i < l; ++i)
            {
                bundleInfo = m_BundleManifest.bundleInfos[i];

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

        public void Clear()
        {
            onInitComplete = null;
            m_AssetInfos.Clear();
            m_AssetBundleInfos.Clear();
        }

        void InitComplete(bool result)
        {
#if ASSETMANAGER_LOG
            Debug.Log("Info Manager init complete");
#endif
            m_Inited = true;
            if (onInitComplete != null)
            {
                onInitComplete(result);
            }
        }

        public bool inited
        {
            get
            {
                return m_Inited;
            }
        }
    }
}
