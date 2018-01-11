using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class InfoManager
    {
        BundleManifest m_BundleManifest;
        Dictionary<string, AssetInfo> m_AssetInfos;
        Dictionary<string, AssetBundleInfo> m_AssetBundleInfos;

        AssetManager m_AssetManager;

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

        IEnumerator LoadPackageFile(string filePath)
        {
            WWW www = new WWW(filePath);
            yield return www;
            bool success = true;
            if (www.error == null)
            {
                using (MemoryStream stream = new MemoryStream(www.bytes))
                {
                    LoadFromStream(stream);
                }
            }
            else
            {
                Debug.LogErrorFormat("LoadPackageFile:{0} error: {1} ", filePath, www.error);
                success = false;
            }

            InitComplete(success);
        }

        public void LoadFromFile(string filePath)
        {
            bool success = true;
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
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

            if(reader.ReadChar()=='A'&& reader.ReadChar() == 'B' && reader.ReadChar()=='M' && reader.ReadChar()=='I')
            {
                LoadFromBinaryStream(steam);
            }
            else
            {
                LoadFromTextStream(steam);
            }

            InitManifest();
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
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
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

        void InitManifest()
        {
            if (m_BundleManifest==null)
            {
                return;
            }

            m_AssetBundleInfos = new Dictionary<string, AssetBundleInfo>();
            m_AssetInfos = new Dictionary<string, AssetInfo>();

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
            if (m_AssetInfos!=null && !string.IsNullOrEmpty(key))
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
            if (m_AssetBundleInfos!=null && m_AssetBundleInfos.ContainsKey(key))
            {
                return m_AssetBundleInfos[key]; 
            }
            return null;
        }

        void InitComplete(bool result)
        {
            Debug.Log("Info Manager init complete");
            if (onInitComplete != null)
            {
                onInitComplete(result);
            }
        }
    }
}
