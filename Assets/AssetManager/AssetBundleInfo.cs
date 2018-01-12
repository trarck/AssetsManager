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

    [System.Serializable]
    public class AssetInfo
    {
        public string fullName;
        /// <summary>
        /// custom define name
        /// </summary>
        public string aliasName;
        public string bundleName;
    }

    [System.Serializable]
    public class AssetBundleInfo 
    {
        public string fullName;
        public string shortName;
        //random name for bundle name.normaly hashName=Hash(fullName)
        //public string hashName;
        public int size;
        public string hash;
        public AssetBundleType type;
        //bundle contents
        public List<AssetInfo> assets;

        //need other bundle
        public string[] dependencies;

        public void Read(BinaryReader reader)
        {
            fullName = reader.ReadString();
            shortName = reader.ReadString();
            size = reader.ReadInt32();
            hash = reader.ReadString();
            type =( AssetBundleType) reader.ReadInt32();

            int assetsCount = reader.ReadInt32();
            assets = new List<AssetInfo>();
            for (int i = 0; i < assetsCount; ++i)
            {
                AssetInfo assetInfo = new AssetInfo();
                assetInfo.fullName = reader.ReadString();
                assetInfo.aliasName = reader.ReadString();
                assets.Add(assetInfo);
            }

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

            writer.Write(assets.Count);
            for(int i=0,l=assets.Count;i<l;++i)
            {
                writer.Write(assets[i].fullName);
                writer.Write(assets[i].aliasName);
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
