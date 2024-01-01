using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    public class LoaderManager:ILoaderManager
    {
		//正在加载assetbundle的loader
		Dictionary<ulong, AssetBundleAsyncLoader> m_AssetBundleLoadings = new Dictionary<ulong, AssetBundleAsyncLoader>();
		
		//正在加载asset的loader
		Dictionary<ulong, AssetAsyncLoader> m_AssetLoadings = new Dictionary<ulong, AssetAsyncLoader>();

		IRequestManager m_RequestManager;
		IInfoManager m_InfoManager;
		IReferenceManager m_ReferenceManager;

		public IRequestManager requestManager
		{
			get
			{
				return m_RequestManager;
			}
			set
			{
				m_RequestManager = value;
			}
		}

		public IInfoManager infoManager
		{
			get
			{
				return m_InfoManager;
			}
			set
			{
				m_InfoManager = value;
			}
		}

		public IReferenceManager referenceManager
		{
			get
			{
				return m_ReferenceManager;
			}
			set
			{
				m_ReferenceManager = value;
			}
		}

		public void Init()
		{
			
		}

		public void Init(IInfoManager infoManager, IRequestManager requestManager, IReferenceManager referenceManager)
		{
			m_InfoManager = infoManager;
			m_RequestManager = requestManager;
			m_ReferenceManager = referenceManager;
		}

		public void Clean()
		{
			m_AssetLoadings.Clear();
			m_AssetBundleLoadings.Clear();
		}

		#region Asset

		public AssetLoadInfo CreateAssetLoadInfo(string path, ulong pathHash)
        {
			AssetLoadInfo info = null;

#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
			info = infoManager.GetAssetInfo(path);
			//can't find asset info
			if (info == null)
			{
				AMDebug.LogWarningFormat("[LoaderManage]Can't find asset info {0}. Load from resource.", path);
				info = new AssetLoadInfo();
				info.path = path;
				if (pathHash != 0)
				{
					info.pathHash = pathHash;
				}
				else if (!string.IsNullOrEmpty(path))
				{
					info.pathHash = xxHash.xxHash64.ComputeHash(path);
				}
			}
#else
            info = new AssetLoadInfo();
            info.path = path;
			if (pathHash != 0)
            {
				info.pathHash = pathHash;
			}
			else if (!string.IsNullOrEmpty(path))
            {
				info.pathHash = xxHash.xxHash64.ComputeHash(path);
			}
#endif
			return info;
		}

		public AssetAsyncLoader CreateAssetAsyncLoader(string path, ulong pathHash=0)
        {
			AssetAsyncLoader loader = null;
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            loader = LoaderPool.AssetAsyncLoaderPool.Get();// new AssetAsyncLoader();
#else
            loader = new AssetEditorLoader();
#endif
			loader.info = CreateAssetLoadInfo(path, pathHash);
            loader.loaderManager = this;
            return loader;
        }

		public AssetSyncLoader CreateAssetSyncLoader(string path, ulong pathHash=0)
		{
			AssetSyncLoader loader = null;
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
			loader = new AssetSyncLoader();
#else
            loader = new AssetEditorLoader();
#endif
			loader.info = CreateAssetLoadInfo(path, pathHash);
			loader.loaderManager = this;
			return loader;
		}

		public AssetAsyncLoader CreateAssetCacheLoader(string path)
		{
			AssetAsyncCacheLoader loader = LoaderPool.AssetAsyncExistLoaderPool.Get();
			loader.loaderManager = this;
			return loader;
		}

		public AssetAsyncLoader CreateOrGetAssetAsyncLoader(string path, int tag, Type type, bool autoReleaseBundle = true)
		{
			AssetAsyncLoader loader = null;
			if (string.IsNullOrEmpty(path))
			{
				return loader;
			}

			ulong assetPathHash = xxHash.xxHash64.ComputeHash(path);

			//1. 检查是不是加载完成。
			AssetReference ar = null;
			if (m_ReferenceManager.TryGetAsset(assetPathHash, out ar))
			{
				//1.1使用已加载的资源
                AMDebug.LogFormat("[LoaderManage]CreateAssetAsyncLoader  using loaded loader  {0}", path);
				//refresh tag
				ar.AddTag(tag);

				loader = CreateAssetCacheLoader(path);
				loader.result = ar;
				loader.autoReleaseBundle = autoReleaseBundle;
				//加载完成后由AssetManager释放loader
				loader.onAfterComplete += OnAssetAfterLoaded;
				return loader;
			}

			//2. 检查是不是有正在loading
			if (m_AssetLoadings.TryGetValue(assetPathHash, out loader))
			{
				//2.1 使用正在加载的loader
				AMDebug.LogFormat("[LoaderManage]CreateAssetAsyncLoader using loading loader {0}", path);
				//资源的名子是唯一的。所以类型也要唯一。
				if (loader.type != type)
				{
					AMDebug.LogErrorFormat(
						"[LoaderManage]CreateAssetAsyncLoader asset {0} is loading.But loading type={1} different with current type={2}",
						path, loader.type, type);
				}

                if (loader.autoReleaseBundle != autoReleaseBundle)
                {
					AMDebug.LogErrorFormat(
					  "[LoaderManage]CreateAssetAsyncLoader asset {0} is loading. " +
					  "But parameter autoReleaseBundle={1} different with current autoReleaseBundle={2},wil be override.",
					  path, loader.type, type);
				}
			}
			else
			{
				//3 创建新的loader
				AMDebug.LogFormat("[LoaderManage]CreateAssetAsyncLoader create new loader {0}", path);
				loader = CreateAssetAsyncLoader(path, assetPathHash);
				m_AssetLoadings[assetPathHash] = loader;

				//对加载前后做特殊处理。只要处理一次。
				loader.Init(OnAssetBeforeLoaded, OnAssetAfterLoaded);

				if (type != null)
				{
					loader.type = type;
				}
			}

			loader.AddParamTag(tag);

			loader.autoReleaseBundle = autoReleaseBundle;

			return loader;
		}

		public AssetLoaderOperation LoadAssetAsync(string path, int tag, Type type, bool autoReleaseBundle,
			Action<AssetReference> completeHandle = null,
			Action<AssetLoader> beforLoadComplete = null,
			Action<AssetLoader> afterLoadComplete = null)
		{
			AssetAsyncLoader loader = CreateOrGetAssetAsyncLoader(path, tag, type, autoReleaseBundle);
			if (loader != null)
			{
				if (completeHandle != null)
				{
					loader.onComplete += completeHandle;
				}

				if (beforLoadComplete != null)
				{
					loader.onBeforeComplete += beforLoadComplete;
				}

				if (afterLoadComplete != null)
				{
					loader.onAfterComplete += afterLoadComplete;
				}

				//这里不在做状态检查，交给loader自己处理。
				ActiveLoader(loader);
			}
			else if (completeHandle != null)
			{
				completeHandle(null);
			}

			return new AssetLoaderOperation(loader);
		}

		public AssetReference LoadAssetSync(string path, int tag, Type type)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			AssetReference ar = null;

			AssetLoader loader = null;

			ulong assetPathHash = xxHash.xxHash64.ComputeHash(path);

			if (m_ReferenceManager.TryGetAsset(assetPathHash, out ar))
			{
				AMDebug.LogFormat("[LoaderManage]LoadAssetSync asset is loaded {0}", path);
				//refresh
				ar.AddTag(tag);

				//cache asset
				ar.Cache();
			}
			else
			{
				if (IsAssetLoading(assetPathHash))
				{
					AMDebug.LogFormat("[LoaderManage]LoadAssetSync async load staring {0}", path);
					//TODO Stop async loader
					return null;
				}
				else
				{
					AMDebug.LogFormat("[LoaderManage]LoadAssetSync create new loader {0}", path);
					loader = CreateAssetSyncLoader(path);
				}

				loader.AddParamTag(tag);

				if (type != null)
				{
					loader.type = type;
				}
				loader.state = Loader.State.Inited;
				loader.Start();
				ar = loader.result;
				OnAssetBeforeLoaded(loader);
				OnAssetAfterLoaded(loader);
			}

			return ar;
		}


		public AssetAsyncLoader CreateOrGetAssetAsyncLoader(ulong assetPathHash, int tag, Type type, bool autoReleaseBundle = true)
		{
			AssetAsyncLoader loader = null;
			if (assetPathHash==0)
			{
				return loader;
			}

			//1. 检查是不是加载完成。
			AssetReference ar = null;
			if (m_ReferenceManager.TryGetAsset(assetPathHash, out ar))
			{
				//1.1使用已加载的资源
				AMDebug.LogFormat("[LoaderManage]CreateAssetAsyncLoader  using loaded loader  {0}", assetPathHash);
				//refresh tag
				ar.AddTag(tag);

				loader = CreateAssetCacheLoader(null);
				loader.result = ar;
				loader.autoReleaseBundle = autoReleaseBundle;
				//加载完成后由AssetManager释放loader
				loader.onAfterComplete += OnAssetAfterLoaded;
				return loader;
			}

			//2. 检查是不是有正在loading
			if (m_AssetLoadings.TryGetValue(assetPathHash, out loader))
			{
				//2.1 使用正在加载的loader
				AMDebug.LogFormat("[LoaderManage]CreateAssetAsyncLoader using loading loader {0}", assetPathHash);
				//资源的名子是唯一的。所以类型也要唯一。
				if (loader.type != type)
				{
					AMDebug.LogErrorFormat(
						"[LoaderManage]CreateAssetAsyncLoader asset {0} is loading.But loading type={1} different with current type={2}",
						assetPathHash, loader.type, type);
				}

				if (loader.autoReleaseBundle != autoReleaseBundle)
				{
					AMDebug.LogErrorFormat(
					  "[LoaderManage]CreateAssetAsyncLoader asset {0} is loading. " +
					  "But parameter autoReleaseBundle={1} different with current autoReleaseBundle={2},wil be override.",
					  assetPathHash, loader.type, type);
				}
			}
			else
			{
				//3 创建新的loader
				AMDebug.LogFormat("[LoaderManage]CreateAssetAsyncLoader create new loader {0}", assetPathHash);
				loader = CreateAssetAsyncLoader(null, assetPathHash);
				m_AssetLoadings[assetPathHash] = loader;

				//对加载前后做特殊处理。只要处理一次。
				loader.Init(OnAssetBeforeLoaded, OnAssetAfterLoaded);

				if (type != null)
				{
					loader.type = type;
				}
			}

			loader.AddParamTag(tag);

			loader.autoReleaseBundle = autoReleaseBundle;

			return loader;
		}

		public AssetLoaderOperation LoadAssetAsync(ulong assetPathHash, int tag, Type type, bool autoReleaseBundle,
			Action<AssetReference> completeHandle = null,
			Action<AssetLoader> beforLoadComplete = null,
			Action<AssetLoader> afterLoadComplete = null)
		{
			AssetAsyncLoader loader = CreateOrGetAssetAsyncLoader(assetPathHash, tag, type, autoReleaseBundle);
			if (loader != null)
			{
				if (completeHandle != null)
				{
					loader.onComplete += completeHandle;
				}

				if (beforLoadComplete != null)
				{
					loader.onBeforeComplete += beforLoadComplete;
				}

				if (afterLoadComplete != null)
				{
					loader.onAfterComplete += afterLoadComplete;
				}

				//这里不在做状态检查，交给loader自己处理。
				ActiveLoader(loader);
			}
			else if (completeHandle != null)
			{
				completeHandle(null);
			}

			return new AssetLoaderOperation(loader);
		}

		public AssetReference LoadAssetSync(ulong assetPathHash, int tag, Type type)
		{
			if (assetPathHash == 0)
			{
				return null;
			}

			AssetReference ar = null;

			AssetLoader loader = null;

			if (m_ReferenceManager.TryGetAsset(assetPathHash, out ar))
			{
				AMDebug.LogFormat("[LoaderManage]LoadAssetSync asset is loaded {0}", assetPathHash);
				//refresh
				ar.AddTag(tag);

				//cache asset
				ar.Cache();
			}
			else
			{
				if (IsAssetLoading(assetPathHash))
				{
					AMDebug.LogFormat("[LoaderManage]LoadAssetSync async load staring {0}", assetPathHash);
					//TODO Stop async loader
					return null;
				}
				else
				{
					AMDebug.LogFormat("[LoaderManage]LoadAssetSync create new loader {0}", assetPathHash);
					loader = CreateAssetSyncLoader(null, assetPathHash);
				}

				loader.AddParamTag(tag);

				if (type != null)
				{
					loader.type = type;
				}
				loader.state = Loader.State.Inited;
				loader.Start();
				ar = loader.result;
				OnAssetBeforeLoaded(loader);
				OnAssetAfterLoaded(loader);
			}

			return ar;
		}


		public void RemoveAssetLoading(AssetLoader loader)
		{
			AssetLoadInfo info = loader.info;
			if (info != null)
			{
				//remove from loading
				if (m_AssetLoadings.ContainsKey(info.pathHash))
				{
					m_AssetLoadings.Remove(info.pathHash);
				}
			}
			else
			{
				ulong key = 0;
				foreach (var iter in m_AssetLoadings)
				{
					if (iter.Value == loader)
					{
						key = iter.Key;
					}
				}

				if (key!=0)
				{
					m_AssetLoadings.Remove(key);
				}
			}
		}

		public bool IsAssetLoading(ulong pathHash)
		{
			return m_AssetLoadings.ContainsKey(pathHash);
		}

		public void OnAssetBeforeLoaded(AssetLoader loader)
		{
			AssetReference ar = loader.result;
			m_ReferenceManager.AddAssetReference(ar);

			//remove from loading
			RemoveAssetLoading(loader);
		}

		public void OnAssetAfterLoaded(AssetLoader loader)
		{
			if (loader.autoReleaseBundle)
			{
				AssetReference ar = loader.result;
				if (ar != null)
				{
					ar.ReleaseBundleReference();
				}
			}

			ReleaseLoader(loader);
		}

		#endregion

		#region AssetBundle
		public AssetBundleLoadInfo CreateAssetBundleLoaderInfo(ulong bundleId)
		{
			AssetBundleLoadInfo info = null;
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
			info = infoManager.GetAssetBundleInfo(bundleId);
			if (info == null)
			{
				AMDebug.LogErrorFormat("[LoaderManage]Can't find asset bundle info {0}", bundleId);
				return null;
			}
#else
            //just for message
            info = new AssetBundleInfo();
            info.bundleId = path;
#endif
			return info;
		}

		public AssetBundleAsyncLoader CreateNewAssetBundleAsyncLoader(ulong bundleId)
        {
			AssetBundleAsyncLoader loader = null;
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            loader = LoaderPool.AssetBundleAsyncLoaderPool.Get();
#else
            loader = new AssetBundleEmptyLoader();
#endif
			loader.info = CreateAssetBundleLoaderInfo(bundleId);
            loader.loaderManager = this;
            return loader;
        }

		public AssetBundleAsyncLoader CreateAssetBundleCacheLoader(ulong bundleId)
		{
			AssetBundleAsyncCacheLoader loader = LoaderPool.AssetBundleAsyncExistLoaderPool .Get();
			loader.loaderManager = this;
			return loader;
		}

		public AssetBundleAsyncLoader CreateAssetBundleAsyncLoader(ulong bundleId, int tag, bool cache)
		{
			AssetBundleAsyncLoader loader = null;

			if (bundleId==0)
			{
				return loader;
			}

			AssetBundleReference abr = null;
			if (m_ReferenceManager.TryGetAssetBundle(bundleId, out abr))
			{
				//asset bundle is loaded
                AMDebug.LogFormat("[LoaderManage]LoadAssetBundle asset bundle is loaded {0}",  bundleId );
				//refresh tag
				abr.AddTag(tag);

				//cache abr
				if (cache)
				{
					abr.Cache();
				}

				//create call back loader
				loader = CreateAssetBundleCacheLoader(bundleId);
				loader.result = abr;

				loader.onAfterComplete += OnAssetBundleAfterLoaded;
			}
			else
			{
				if (!m_AssetBundleLoadings.TryGetValue(bundleId, out loader))
				{
                    AMDebug.LogFormat("[LoaderManage]LoadAssetBundle create new loader {0}" , bundleId );
					loader = CreateNewAssetBundleAsyncLoader(bundleId);
					if (loader != null)
					{
						m_AssetBundleLoadings[bundleId] = loader;
					}
					else
					{
						return null;
					}

					//对加载前后做特殊处理。只要处理一次。
					loader.Init(OnAssetBundleBeforeLoaded, OnAssetBundleAfterLoaded);
				}
				else
				{
					AMDebug.LogFormat("[LoaderManage]LoadAssetBundle using loading loader {0}" , bundleId);
				}

				loader.AddParamTag(tag);

				loader.SetCacheResult(cache);
			}
			return loader;
		}

		public AssetBundleSyncLoader CreateAssetBundleSyncLoader(ulong bundleId)
		{
			AssetBundleSyncLoader loader = null;

#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
			loader = LoaderPool.AssetBundleSyncLoaderPool.Get();
#else
			AMDebug.LogWarningFormat("[LoaderManager]Not need  load bundle {0}, create empty", path);
            loader = new AssetBundleEmptyLoader();
#endif
			loader.info = CreateAssetBundleLoaderInfo(bundleId);
			loader.loaderManager = this;
			return loader;
		}

		public AssetBundleLoader LoadAssetBundleAsync(ulong bundleId, int tag, bool cache,
			Action<AssetBundleReference> completeHandle = null,
			Action<AssetBundleLoader> beforLoadComplete = null,
			Action<AssetBundleLoader> afterLoadComplete = null)
		{
			AssetBundleLoader loader = CreateAssetBundleAsyncLoader(bundleId, tag, cache);

			if (loader != null)
			{
				if (completeHandle != null)
				{
					loader.onComplete += completeHandle;
				}

				if (beforLoadComplete != null)
				{
					loader.onBeforeComplete += beforLoadComplete;
				}

				if (afterLoadComplete != null)
				{
					loader.onAfterComplete += afterLoadComplete;
				}

				//这里不在做状态检查，交给loader自己处理。
				ActiveLoader(loader);
			}
			else if (completeHandle != null)
			{
				completeHandle(null);
			}

			return loader;
		}


		public AssetBundleReference LoadAssetBundleSync(ulong bundleId, int tag, bool cache = true)
		{
			if (bundleId==0)
			{
				return null;
			}

			AssetBundleReference abr = null;

			if (m_ReferenceManager.TryGetAssetBundle(bundleId, out abr))
			{
				AMDebug.LogFormat("[LoaderManage]LoadAssetBundleSync bundle is loaded {0}", bundleId);
				//refresh 
				abr.AddTag(tag);

				if (cache)
				{
					abr.Cache();
				}
			}
			else
			{
				if (IsAssetBundleLoading(bundleId))
				{
					AMDebug.LogErrorFormat("[LoaderManage]LoadAssetBundleSync async loader is active {0}", bundleId);
					//TODO Stop async
					return null;
				}
				else
				{
					AMDebug.LogFormat("[LoaderManage]LoadAssetBundleSync create new loader {0}", bundleId);
					AssetBundleSyncLoader loader = CreateAssetBundleSyncLoader(bundleId);
					if (loader != null)
					{
						loader.state = Loader.State.Inited;
						loader.SetCacheResult(cache);

						loader.Start();
						abr = loader.result;
						//must retain . will be destory by loader clean
						abr.Retain();
						OnAssetBundleBeforeLoaded(loader);
						OnAssetBundleAfterLoaded(loader);
					}
				}
			}

			return abr;
		}


		public void RemoveAssetBundleLoading(AssetBundleLoader loader)
		{
			AssetBundleLoadInfo info = loader.info;
			if (info != null)
			{
				if (m_AssetBundleLoadings.ContainsKey(info.bundleId))
				{
					m_AssetBundleLoadings.Remove(info.bundleId);
				}
			}
			else
			{
				ulong key = 0;
				foreach (var iter in m_AssetBundleLoadings)
				{
					if (iter.Value == loader)
					{
						key = iter.Key;
					}
				}

				if (key>0)
				{
					m_AssetBundleLoadings.Remove(key);
				}
			}

		}

		public bool IsAssetBundleLoading(ulong bundleId)
		{
			return m_AssetBundleLoadings.ContainsKey(bundleId);
		}

		public void OnAssetBundleBeforeLoaded(AssetBundleLoader loader)
		{
			AssetBundleReference abr = loader.result;
			m_ReferenceManager.AddAssetBundleReference(abr, loader.cacheResult);

			RemoveAssetBundleLoading(loader);
		}

		public void OnAssetBundleAfterLoaded(AssetBundleLoader loader)
		{
			ReleaseLoader(loader);
		}
		#endregion

		public void ActiveLoader(Loader loader)
        {
			loader.IncreaseLoadingCount();
            loader.Start();
            //if (m_ActiveLoaders.Count < m_MaxActiveLoader)
            //{
            //    m_ActiveLoaders.Add(loader);
            //    loader.Start();
            //}
            //else
            //{
            //    m_PrepareLoaders.Push(loader);
            //}
        }

        public void ReleaseLoader(Loader loader)
        {
			if (loader.isEmptyRef)
			{
				LoaderPool.Release(loader);
			}
		}
    }
}
