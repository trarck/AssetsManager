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

		public static readonly ObjectPool<AssetBundleAsyncExistLoader> AssetBundleAsyncExistLoaderPool = new ObjectPool<AssetBundleAsyncExistLoader>(null, l => l.Clean());
		public static readonly ObjectPool<AssetAsyncExistLoader> AssetAsyncExistLoaderPool = new ObjectPool<AssetAsyncExistLoader>(null, l => l.Clean());

		private static Dictionary<Type, Action<Loader>> m_ReleaseLoaderTypeMap = InitReleaseLoaderTypeMap();

		private static Dictionary<Type, Action<Loader>> InitReleaseLoaderTypeMap()
		{
			return new Dictionary<Type, Action<Loader>>(){
				{typeof(AssetBundleAsyncLoader),ReleaseAssetBundleAsyncLoader},
				{typeof(AssetAsyncLoader),ReleaseAssetAsyncLoader},
				{typeof(AssetBundleSyncLoader),ReleaseAssetBundleSyncLoader},
				{typeof(AssetSyncLoader),ReleaseAssetSyncLoader},
				{typeof(AssetBundleAsyncExistLoader),ReleaseAssetBundleAsyncExistLoader},
				{typeof(AssetAsyncExistLoader),ReleaseAssetAsyncExistLoader}
			};	  
		}

		public static void ReleaseAssetBundleAsyncLoader(Loader loader)
		{
			AssetBundleAsyncLoaderPool.Release(loader as AssetBundleAsyncLoader);
		}

		public static void ReleaseAssetAsyncLoader(Loader loader)
		{
			AssetAsyncLoaderPool.Release(loader as AssetAsyncLoader);
		}

		public static void ReleaseAssetBundleSyncLoader(Loader loader)
		{
			AssetBundleSyncLoaderPool.Release(loader as AssetBundleSyncLoader);
		}

		public static void ReleaseAssetSyncLoader(Loader loader)
		{
			AssetSyncLoaderPool.Release(loader as AssetSyncLoader);
		}

		public static void ReleaseAssetBundleAsyncExistLoader(Loader loader)
		{
			AssetBundleAsyncExistLoaderPool.Release(loader as AssetBundleAsyncExistLoader);
		}

		public static void ReleaseAssetAsyncExistLoader(Loader loader)
		{
			AssetAsyncExistLoaderPool.Release(loader as AssetAsyncExistLoader);
		}

		public static void Release(Loader loader)
		{
			
			Type t = loader.GetType();
			Action<Loader> act = null;
			if (m_ReleaseLoaderTypeMap.TryGetValue(t, out act))
			{
				act(loader);
			}
			else
			{
				loader.Clean();
			}

			//if (loader is AssetBundleAsyncExistLoader)
			//{
			//	AssetBundleAsyncExistLoaderPool.Release(loader as AssetBundleAsyncExistLoader);
			//}
			//else if (loader is AssetAsyncExistLoader)
			//{
			//	AssetAsyncExistLoaderPool.Release(loader as AssetAsyncExistLoader);
			//}
			//else if (loader is AssetAsyncLoader)
			//{
			//	AssetAsyncLoaderPool.Release(loader as AssetAsyncLoader);
			//}
			//else if (loader is AssetBundleAsyncLoader)
			//{
			//	AssetBundleAsyncLoaderPool.Release(loader as AssetBundleAsyncLoader);
			//}
			//else if (loader is AssetBundleSyncLoader)
			//{
			//	AssetBundleSyncLoaderPool.Release(loader as AssetBundleSyncLoader);
			//}
			//else if (loader is AssetSyncLoader)
			//{
			//	AssetSyncLoaderPool.Release(loader as AssetSyncLoader);
			//}
			//else
			//{
			//	loader.Clean();
			//}
        }        
    }
}
