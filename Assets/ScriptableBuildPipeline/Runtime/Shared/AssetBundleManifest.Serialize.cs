using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if false
namespace YH.AssetManage
{
    public class VersionSerializer
    {
        public static void SerializeVersion(Version version, BinaryWriter writer)
        {
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Build);
            writer.Write(version.Revision);
        }

        public static Version DeserializeVersion(BinaryReader reader)
        {
            int major = reader.ReadInt32();
            int minor = reader.ReadInt32();
            int build = reader.ReadInt32();
            int revision = reader.ReadInt32();
            Version version = new Version(major, minor, build, revision);
            return version;
        }
    }

    public enum ManifestFlag : ushort
    {
        None=0,
        BundleDependenciesAll = 1,
    }

    public struct AssetBundleManifestHeader
    {
        //little endian
        public const uint Magic = 0x494d4241;//ABMI
        public const ushort Format = 1;

        public uint magic;//0x494d4241;
        public byte format; //1
        public ManifestFlag flag;
        public byte blockCount;
    }

    public struct AssetBundleManifestBlockInfo
    {
        public BlockType type;
        public uint offset;

        public uint SerializeValue()
        {
            return (uint)type << 24 | (offset & 0xFFFFFF);
        }

        public void DeserializeValue(uint val)
        {
            type = (BlockType)(val >> 24);
            offset = val & 0xFFFFFF;
        }
    }

    public enum BlockType
    {
        Version = 1,
        Bundle = 2,
    }

    public abstract class AssetBundleManifestSerialzer
    {
        public const uint Magic = 0x4145564d;
        public const byte Format = 1;

        protected AssetBundleManifestHeader _Header;
        protected AssetBundleManifestBlockInfo[] _BlockTable;
    }
}
#endif