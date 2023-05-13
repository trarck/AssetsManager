using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YH.AssetManage
{
    public class AssetBundleManifestReader : AssetBundleManifestSerialzer
    {
        protected BinaryReader _Reader;

        public BinaryReader Reader
        {
            get { return _Reader; }
        }

        public AssetBundleManifestReader(Stream stream)
        {
            _Reader = new BinaryReader(stream, Encoding.Default, true);
        }

        public void ReadManifest(ref AssetBundleManifest manifest)
        {
            //read head
            ReadHeader();

            //read block tabel
            ReadBlockTable();

            //read data
            for (int i = 0; i < _Header.blockCount; ++i)
            {
                switch (_BlockTable[i].type)
                {
                    case BlockType.Version:
                    {
                        //asset version
                        ReadVersion(manifest);
                        break;
                    }
                    case BlockType.Bundle:
                    {
                        //bundles
                        ReadBundles(manifest);
                        break;
                    }
                }
            }
        }

        protected void ReadHeader()
        {
            _Header = new AssetBundleManifestHeader();
            _Header.magic = _Reader.ReadUInt32();
            _Header.format = _Reader.ReadByte();
            _Header.flag =(ManifestFlag) _Reader.ReadUInt16();
            _Header.blockCount = _Reader.ReadByte();
        }

        protected void ReadBlockTable()
        {
            if (_Header.blockCount == 0)
            {
                //no data to read
                return;
            }

            _BlockTable = new AssetBundleManifestBlockInfo[_Header.blockCount];
            for(int i = 0; i < _Header.blockCount; ++i)
            {
                _BlockTable[i].DeserializeValue(_Reader.ReadUInt32());
            }
        }

        protected void ReadVersion(AssetBundleManifest manifest)
        {
            manifest.version = VersionSerializer.DeserializeVersion(_Reader);
        }

        protected void ReadBundles(AssetBundleManifest manifest)
        {
            int count = _Reader.ReadInt32();
            manifest._AssetIdToBundles = new Dictionary<ulong, AssetBundleInfo>(count * 2);
            manifest._Bundles = new Dictionary<ulong, AssetBundleInfo>(count);

            List<AssetBundleInfo> bundles = new List<AssetBundleInfo>(count);
            for (int i = 0; i < count; ++i)
            {
                AssetBundleInfo bundleInfo = new AssetBundleInfo();
                bundleInfo.id = _Reader.ReadUInt64();
                ReadAssets(manifest, bundleInfo);
                manifest._Bundles.Add(bundleInfo.id, bundleInfo);
                bundles.Add(bundleInfo);
            }

            for (int i = 0; i < count; ++i)
            {
                AssetBundleInfo bundleInfo = bundles[i];
                ReadDependencies(bundles, bundleInfo);
            }
        }

        protected void ReadAssets(AssetBundleManifest manifest, AssetBundleInfo bundleInfo)
        {
            int count = _Reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                ulong assetId = _Reader.ReadUInt64();
                manifest._AssetIdToBundles[assetId] = bundleInfo;
            }
        }

        protected void ReadDependencies(List<AssetBundleInfo> bundles, AssetBundleInfo bundleInfo)
        {
            int depCount = _Reader.ReadInt32();
            bundleInfo.dependencies = new AssetBundleInfo[depCount];
            for (int i = 0; i < depCount; ++i)
            {
                int bundleIndex = _Reader.ReadInt32();
                bundleInfo.dependencies[i] = bundles[bundleIndex];
            }
        }
    }
}
