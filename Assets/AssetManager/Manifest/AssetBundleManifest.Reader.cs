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
            ReadHeader(manifest);
   
            //read stream block tabel
            ReadStreamBlockTable();

            //read data
            // TODO use regiest Block read function
            for (int i = 0; i < _Header.streamBlockCount; ++i)
            {
                switch (_StreamBlockTable[i].type)
                {
                    case StreamBlockType.Version:
                    {
                        //asset version
                        ReadVersion(_StreamBlockTable[i], manifest);
                        break;
                    }
                    case StreamBlockType.Bundle:
                    {
                        //bundles
                        ReadBundles(_StreamBlockTable[i], manifest);
                        break;
                    }
                }
            }
        }

        protected void ReadHeader(AssetBundleManifest manifest)
        {
            _Header = new AssetBundleManifestHeader();
            _Header.magic = _Reader.ReadUInt32();
            _Header.format = _Reader.ReadByte();
            _Header.flag =(ManifestFlag) _Reader.ReadByte();
            _Header.streamBlockCount = _Reader.ReadUInt16();

            manifest.format = _Header.format;
            manifest.bundleDependenciesAll = (_Header.flag & ManifestFlag.BundleDependenciesAll) != 0;
            manifest.useBlock = (_Header.flag & ManifestFlag.BundleInBlock) != 0;
        }

        protected void ReadStreamBlockTable()
        {
            if (_Header.streamBlockCount == 0)
            {
                //no data to read
                return;
            }

            _StreamBlockTable = new AssetBundleManifestBlockInfo[_Header.streamBlockCount];
            for(int i = 0; i < _Header.streamBlockCount; ++i)
            {
                _StreamBlockTable[i].DeserializeValue(_Reader.ReadUInt32());
            }
        }

        protected void ReadVersion(AssetBundleManifestBlockInfo blockInfo, AssetBundleManifest manifest)
        {
            //_Reader.BaseStream.Position = blockInfo.offset;
            manifest.version = VersionSerializer.DeserializeVersion(_Reader);
        }

        protected void ReadBundles(AssetBundleManifestBlockInfo blockInfo, AssetBundleManifest manifest)
        {
            if (manifest.useBlock)
            {
                ReadBundlesWithBlock(blockInfo, manifest);
            }
            else
            {
                ReadBundlesNoBlock(blockInfo, manifest);
            }
        }


        protected void ReadBundlesNoBlock(AssetBundleManifestBlockInfo streamBlockInfo, AssetBundleManifest manifest)
        {
            //_Reader.BaseStream.Position = streamBlockInfo.offset;
            int count = _Reader.ReadInt32();
            manifest._AssetIdToBundleIds = new Dictionary<ulong, ulong>(count);
            manifest._Bundles = new Dictionary<ulong, AssetBundleInfo2>(count);

            for (int i = 0; i < count; ++i)
            {
                AssetBundleInfo2 bundleInfo = new AssetBundleInfo2();
                ulong bundleId = _Reader.ReadUInt64();
                manifest._Bundles.Add(bundleId, bundleInfo);
                ReadAssets(bundleId, manifest);
                ReadDependencies(bundleInfo, manifest);
            }
        }

        protected void ReadBundlesWithBlock(AssetBundleManifestBlockInfo streamBlockInfo, AssetBundleManifest manifest)
        {
            //_Reader.BaseStream.Position = streamBlockInfo.offset;
            int count = _Reader.ReadInt32();
            manifest._AssetIdToBundleIds = new Dictionary<ulong, ulong>(count);
            manifest._Bundles = new Dictionary<ulong, AssetBundleInfo2>(count);

            for (int i = 0; i < count; ++i)
            {
                AssetBundleBlockInfo bundleInfo = new AssetBundleBlockInfo();
                ulong bundleId = _Reader.ReadUInt64();
                manifest._Bundles.Add(bundleId, bundleInfo);

                ReadAssets(bundleId, manifest);
                ReadDependencies(bundleInfo, manifest);
                bundleInfo.contentId = _Reader.ReadUInt64();
                bundleInfo.offset = _Reader.ReadUInt32();
            }
        }

        protected void ReadAssets(ulong bundleId, AssetBundleManifest manifest)
        {
            int count = _Reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                ulong assetId = _Reader.ReadUInt64();
                manifest._AssetIdToBundleIds[assetId] = bundleId;
            }
        }

        protected void ReadDependencies(AssetBundleInfo2 bundleInfo, AssetBundleManifest manifest)
        {
            int depCount = _Reader.ReadInt32();
            bundleInfo.dependencies = new ulong[depCount];
            for (int i = 0; i < depCount; ++i)
            {
                bundleInfo.dependencies[i] = _Reader.ReadUInt64();
            }
        }
    }
}
