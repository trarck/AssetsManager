using System;
using System.Collections.Generic;

namespace YH.AssetManage
{
    public class AssetInfo
    {
        //路径hash。
        //序列化的时候是有pathHash。在创建运行时对象时，不加pathHash，pathHash作为字典的key里，value里没有必要在保存一份。
        //public ulong pathHash;
        //asset bundle 内容hash
        public ulong bunldeId;
        //别名hash。支持把路径名作为overide记录到AssetBundle里。
        //public ulong aliasHash;
        //资源路径，以Assets开头的相对路径。
        //public string path;
    }

    public class AssetBundleInfo2
    {
        //bundle path hash use block. bundle content hash not use block
        //序列化的时候是有bundleId。在创建运行时对象时，不加bundleId，bundleId作为字典的key里，value里没有必要在保存一份。预计10万量级可以省8k左右内容
        // 只需要在加载的时候，补上bundleId。同时加载的量级还是少的。
        //public ulong bundleId; 
        //public ulong[] assets;
        //dependencies bundle id. all deps or  direct deps
        public ulong[] dependencies;

        public virtual ulong GetContentId()
        {
            return 0;
        }

        public virtual uint GetOffset()
        {
            return 0;
        }
    }

    public class AssetBundleBlockInfo:AssetBundleInfo2
    {
        //bundle path hash use block. bundle content hash not use block
        //序列化的时候是有bundleId。在创建运行时对象时，不加bundleId，bundleId作为字典的key里，value里没有必要在保存一份。预计10万量级可以省8k左右内容
        // 只需要在加载的时候，补上bundleId。同时加载的量级还是少的。
        //public ulong bundleId; 
        //public ulong[] assets;
        //dependencies bundle id. all deps or  direct deps
        public ulong contentId;
        public uint offset;

        public override ulong GetContentId()
        {
            return contentId;
        }

        public override uint GetOffset()
        {
            return offset;
        }
    }

    public class AssetBundleEditorInfo
    {
        //bundle path hash use block. bundle content hash not use block
        public ulong bundleId; //not use in manifest less memory, but use in load
        public ulong[] assets;
        //dependencies bundle id. all deps or  direct deps
        public ulong[] dependencies;
        public ulong blockId;
        public uint offset;
    }

    public class AssetBundleManifest
    {
        public byte format;
        //bundle  dependencies is all or direct;
        public bool bundleDependenciesAll;
        public bool useBlock;

        public Version version;

        //asset id to bundle info
        internal Dictionary<ulong, ulong> _AssetIdToBundleIds;
        //bundle id to bundle info
        internal Dictionary<ulong, AssetBundleInfo2> _Bundles;
    }

}
