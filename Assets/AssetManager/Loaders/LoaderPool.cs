using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.AssetManage
{
    internal static class LoaderPool
    {
        // Object pool to avoid allocations.
        public static readonly ObjectPool<AssetBundleAsyncLoader> AssetBundleAsyncLoaderPool = new ObjectPool<AssetBundleAsyncLoader>(null, l=>l.Clean());
        public static readonly ObjectPool<AssetAsyncLoader> AssetAsyncLoaderPool = new ObjectPool<AssetAsyncLoader>(null, l => l.Clean());

        public static readonly ObjectPool<AssetBundleSyncLoader> AssetBundleSyncLoaderPool = new ObjectPool<AssetBundleSyncLoader>(null, l => l.Clean());
        public static readonly ObjectPool<AssetSyncLoader> AssetSyncLoaderPool = new ObjectPool<AssetSyncLoader>(null, l => l.Clean());
        public static void Release(Loader loader)
        {
            if (loader is AssetAsyncLoader)
            {
                AssetAsyncLoaderPool.Release(loader as AssetAsyncLoader);
            }
            else if (loader is AssetBundleAsyncLoader)
            {
                AssetBundleAsyncLoaderPool.Release(loader as AssetBundleAsyncLoader);
            }
            else if (loader is AssetBundleSyncLoader)
            {
                AssetBundleSyncLoaderPool.Release(loader as AssetBundleSyncLoader);
            }
            else if (loader is AssetSyncLoader)
            {
                AssetSyncLoaderPool.Release(loader as AssetSyncLoader);
            }
            else
            {
                loader.Clean();
            }
        }        
    }
}
