using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YH.AssetManage
{
    public class AssetLoadInfo
    {
        //路径hash
        public ulong pathHash;
        //asset bundle 内容hash
        public ulong bunldeId;
        ////别名hash。支持把路径名作为overide记录到AssetBundle里。
        //public ulong aliasHash;
        //资源路径，以Assets开头的相对路径。
        public string path;

        public void Clean()
        {
            pathHash = 0;
            bunldeId = 0;
            path = null;
        }
    }

    public class AssetBundleLoadInfo 
    {
        // buindle id
        public ulong bundleId;
        public AssetBundleRuntimeInfo assetBundleInfo;

        public void Clean()
        {
            bundleId = 0;
            assetBundleInfo = null;
        }

        public ulong GetContentId()
        {
            ulong contentId = 0;
            if (assetBundleInfo != null)
            {
                contentId= assetBundleInfo.GetContentId();
                if (contentId == 0)
                {
                    contentId = bundleId;
                }
            }
            return contentId;
        }

        public uint GetOffset()
        {
            if (assetBundleInfo != null)
            {
                return assetBundleInfo.GetOffset();
            }
            return 0;
        }
    }
}
