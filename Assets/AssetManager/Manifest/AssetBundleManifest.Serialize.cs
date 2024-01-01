using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YH.AssetManage
{
    public enum ManifestFlag : byte
    {
        None=0,
        BundleDependenciesAll = 1,
        BundleInBlock = 2
    }

    /// <summary>
    /// 8 byte
    /// </summary>
    public struct AssetBundleManifestHeader
    {
        //little endian
        public const uint Magic = 0x494d4241;//ABMI
        public const byte Format = 1;

        public uint magic;//0x494d4241;   //4b
        public byte format; //1b
        public ManifestFlag flag;   //1b
        public ushort streamBlockCount;    //2b
    }

    public struct AssetBundleManifestStreamInfo
    {
        public StreamType type;//1b
        public uint offset;       //3b

        public uint SerializeValue()
        {
            return (uint)type << 24 | (offset & 0xFFFFFF);
        }

        public void DeserializeValue(uint val)
        {
            type = (StreamType)(val >> 24);
            offset = val & 0xFFFFFF;
        }
    }

    public enum StreamType : byte
    {
        Version = 1,
        Bundle = 2,
    }

    public abstract class AssetBundleManifestSerialzer
    {
        public const uint Magic = 0x494d4241;//ABMI
        public const byte CurrentFormat = 1;

        protected AssetBundleManifestHeader _Header;
        protected AssetBundleManifestStreamInfo[] _StreamBlockTable;
    }

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
            if (build < 0)
            {
                build = 0;
            }
            if (revision < 0)
            {
                revision = 0;
            }
            Version version = new Version(major, minor, build, revision);
            return version;
        }
    }
}
