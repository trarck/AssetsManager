using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public enum AssetBundleType
    {
        StandeAlone=1,
        Reference=2
    }

    [System.Serializable]
    public class AssetBundleInfo 
    {
        public string fullName;
        public string shortName;
        public int size;
        public string hash;
        public AssetBundleType type;
        public string[] dependencies;

        //create by runtime
        List<AssetBundleInfo> m_DependenciesData;

        public List<AssetBundleInfo> GetDependencies()
        {
            return m_DependenciesData;
        }

        public void SetDependencies(List<AssetBundleInfo> dependenciesData)
        {
            m_DependenciesData=dependenciesData;
        }
    }

    [System.Serializable]
    public class BundleManifest
    {
        public string version = "1";
        public List<AssetBundleInfo> bundleInfos;
    }
}
