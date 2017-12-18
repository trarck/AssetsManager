using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YH.AssetManager
{
    public enum AssetBundleType
    {
        StandeAlone=1,
        Reference=2
    }

    public class AssetInfo
    {
        public string name;
        public string bundleName;
    }

    [System.Serializable]
    public class AssetBundleInfo 
    {
        public string fullName;
        public string shortName;
        public int size;
        public string hash;
        public AssetBundleType type;
        //bundle contents
        public string[] assets;

        //need other bundle
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

        public void Read(BinaryReader reader)
        {
            fullName = reader.ReadString();
            shortName = reader.ReadString();
            size = reader.ReadInt32();
            hash = reader.ReadString();
            type =( AssetBundleType) reader.ReadInt32();

            int assetsCount = reader.ReadInt32();
            List<string> assetNames= new List<string>();
            for(int i = 0; i < assetsCount; ++i)
            {
                assetNames.Add(reader.ReadString());
            }
            assets = assetNames.ToArray();

            int dependencyCount = reader.ReadInt32();
            List<string> dependencyNames = new List<string>();
            for (int i = 0; i < dependencyCount; ++i)
            {
                dependencyNames.Add(reader.ReadString());
            }
            dependencies = dependencyNames.ToArray();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(fullName);
            writer.Write(shortName);
            writer.Write(size);
            writer.Write(hash);
            writer.Write((int)type);

            writer.Write(assets.Length);
            for(int i=0,l=assets.Length;i<l;++i)
            {
                writer.Write(assets[i]);
            }

            writer.Write(dependencies.Length);
            for (int i = 0, l = dependencies.Length; i < l; ++i)
            {
                writer.Write(dependencies[i]);
            }
        }
    }

    [System.Serializable]
    public class BundleManifest
    {
        public string version = "1";
        public List<AssetBundleInfo> bundleInfos;

        public void Read(BinaryReader reader)
        {
            version = reader.ReadString();
            int bundleCount = reader.ReadInt32();

            AssetBundleInfo bundleInfo = null;
            for (uint i = 0; i < bundleCount; ++i)
            {
                bundleInfo = new AssetBundleInfo();
                bundleInfo.Read(reader);
            }
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(bundleInfos.Count);
            for(int i = 0, l = bundleInfos.Count; i < l; ++i)
            {
                bundleInfos[i].Write(writer);
            }
        }
    }
}
