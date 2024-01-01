using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.AssetManage
{
    internal static class LoadInfoPool
    {
        // Object pool to avoid allocations.
        public static readonly ObjectPool<AssetLoadInfo> AssetLoadInfoPool = new ObjectPool<AssetLoadInfo>(null, l=>l.Clean());
        public static readonly ObjectPool<AssetBundleLoadInfo> AssetBundleLoadInfoPool = new ObjectPool<AssetBundleLoadInfo>(null, l => l.Clean());
	
		public static AssetLoadInfo GetAssetLoadInfo()
        {
			return AssetLoadInfoPool.Get();
        }

		public static void ReleaseAssetLoadInfo(AssetLoadInfo loadinfo)
		{
			AssetLoadInfoPool.Release(loadinfo);
		}

		public static AssetBundleLoadInfo GetAssetBundleLoadInfo()
		{
			return AssetBundleLoadInfoPool.Get();
		}

		public static void ReleaseAssetBundleLoadInfo(AssetBundleLoadInfo loadinfo)
		{
			AssetBundleLoadInfoPool.Release(loadinfo);
		}
    }
}
