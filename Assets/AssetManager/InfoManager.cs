using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class InfoManager
    {
        BundleManifest m_BundleManifest;
        Dictionary<string, AssetInfo> m_AssetInfos;
        Dictionary<string, AssetBundleInfo> m_AssetBundleInfos;

        public void LoadFromFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            m_BundleManifest = JsonUtility.FromJson<BundleManifest>(content);
            InitManifest();
        }

        public void LoadFromBinary(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                LoadFromStream(fs);
            }
        }

        public void LoadFromStream(Stream steam)
        {
            BinaryReader reader = new BinaryReader(steam);
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

                //create bundle dependes
                CreateBundleDependencies(bundleInfo);

                //create asset info map
                for (int j = 0, k = bundleInfo.assets.Length; j < k; ++j)
                {
                    string assetName = bundleInfo.assets[j];
                    assetInfo = new AssetInfo();
                    assetInfo.name = assetName;
                    assetInfo.bundleName = bundleInfo.fullName;
                    m_AssetInfos.Add(assetName, assetInfo);
                }
            }
        }

        public AssetBundleInfo Find(string key)
        {
            if (m_AssetInfos.ContainsKey(key))
            {
                return m_AssetBundleInfos[key]; 
            }
            return null;
        }

        protected void CreateBundleDependencies(AssetBundleInfo info)
        {
            if (info.GetDependencies() == null)
            {
                string[] dependencies = info.dependencies;
                List<AssetBundleInfo> data = new List<AssetBundleInfo>();
                for(int i=0,l=dependencies.Length;i< l; ++i)
                {
                    if (m_AssetBundleInfos.ContainsKey(dependencies[i]))
                    {
                        data.Add(m_AssetBundleInfos[dependencies[i]]);
                    }
                    else
                    {
                        throw new Exception("Can't find dependency info of " + dependencies[i]);
                    }
                }
            }
        }
    }
}
