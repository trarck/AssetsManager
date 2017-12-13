using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetInfoManager
    {
        Dictionary<string, AssetBundleInfo> m_AssetInfos;

        public void LoadFromFile(string filePath)
        {

        }

        public AssetBundleInfo Find(string key)
        {
            if (m_AssetInfos.ContainsKey(key))
            {
                AssetBundleInfo info= m_AssetInfos[key];
                CreateInfoDependencies(info);
                return info;
            }
            return null;
        }

        public void CreateInfoDependencies(AssetBundleInfo info)
        {
            if (info.GetDependencies() == null)
            {
                string[] dependencies = info.dependencies;
                List<AssetBundleInfo> data = new List<AssetBundleInfo>();
                for(int i=0,l=dependencies.Length;i< l; ++i)
                {
                    if (m_AssetInfos.ContainsKey(dependencies[i]))
                    {
                        data.Add(m_AssetInfos[dependencies[i]]);
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
