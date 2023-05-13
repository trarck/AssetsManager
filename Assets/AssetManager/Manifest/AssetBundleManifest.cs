using System;
using System.Collections.Generic;

namespace YH.AssetManage
{
    public class AssetBundleInfo
    {
        //bundle content hash
        public ulong id;
        //dependencies bundle id. all deps or  direct deps
        public AssetBundleInfo[] dependencies;
    }

    public class AssetBundleManifest
    {
        //bundle  dependencies is all or direct;
        public bool bundleDependenciesAll;

        public Version version;

        //asset id to bundle info
        internal Dictionary<ulong, AssetBundleInfo> _AssetIdToBundles;
        //bundle id to bundle info
        internal Dictionary<ulong, AssetBundleInfo> _Bundles;
    }

}
