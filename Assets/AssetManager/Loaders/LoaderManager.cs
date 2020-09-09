using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    public class LoaderManager:ILoaderManager
    {
		//正在加载assetbundle的loader
		Dictionary<string, AssetBundleAsyncLoader> m_AssetBundleLoadings = new Dictionary<string, AssetBundleAsyncLoader>();
		
		//正在加载asset的loader
		Dictionary<string, AssetAsyncLoader> m_AssetLoadings = new Dictionary<string, AssetAsyncLoader>();

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

		public AssetLoader LoadAssetAsync(string path, int tag, Type type, bool autoReleaseBundle,
			Action<AssetReference> completeHandle = null,
			Action<AssetLoader> beforLoadComplete = null,
			Action<AssetLoader> afterLoadComplete = null)
		{
			AssetAsyncLoader loader = CreateSingleAssetAsyncLoader(path, tag, type, autoReleaseBundle);
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

		public AssetReference LoadAssetSync(string path, int tag, Type type)
		{
			if (!string.IsNullOrEmpty(path))
			{
				path = AssetPaths.AddAssetPrev(path);
			}

			AssetReference ar = null;

			AssetLoader loader = null;

			if (m_ReferenceManager.TryGetAsset(path, out ar))
			{
#if ASSETMANAGER_LOG_ON
                Debug.Log("[AssetManage]LoadAssetSync asset is loaded " + path + "," + Time.frameCount);
#endif
				//refresh
				ar.AddTag(tag);

				//cache asset
				ar.Cache();
			}
			else
			{
				if (IsAssetLoading(path))
				{
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]LoadAssetSync async load staring " + path + "," + Time.frameCount);
#endif
					//TODO Stop async loader
					return null;
				}
				else
				{
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]LoadAssetSync create new loader " + path + "," + Time.frameCount);
#endif
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

		public AssetAsyncLoader CreateAssetAsyncLoader(string path)
        {
			AssetAsyncLoader loader = null;
            AssetInfo info = null;

#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            info = infoManager.FindAssetInfo(path);
            //can't find asset info
            if (info == null)
            {
                Debug.LogErrorFormat("[AssetManage]Can't find asset info {0}.Load from resource.", path);
                info = new AssetInfo();
                info.fullName = path;
            }

            loader = LoaderPool.AssetAsyncLoaderPool.Get();// new AssetAsyncLoader();
#else
            loader = new AssetEditorLoader();
            info = new AssetInfo();
            info.fullName = path;
#endif
            loader.info = info;
            loader.loaderManager = this;
            return loader;
        }

		public AssetSyncLoader CreateAssetSyncLoader(string path)
		{
			AssetSyncLoader loader = null;
			AssetInfo info = null;

#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
			info = infoManager.FindAssetInfo(path);
			//can't find asset info
			if (info == null)
			{
				info = new AssetInfo();
				info.fullName = path;
			}

			loader = new AssetSyncLoader();
#else
            loader = new AssetSyncLoader();
            info = new AssetInfo();
            info.fullName = path;
#endif
			loader.info = info;
			loader.loaderManager = this;
			return loader;
		}

		public AssetAsyncLoader CreateAssetExistLoader(string path)
		{
			AssetAsyncExistLoader loader = LoaderPool.AssetAsyncExistLoaderPool.Get();
			loader.loaderManager = this;
			return loader;
		}

		public AssetAsyncLoader CreateSingleAssetAsyncLoader(string path, int tag, Type type, bool autoReleaseBundle = true)
		{
			AssetAsyncLoader loader = null;
			if (!string.IsNullOrEmpty(path))
			{
				path = AssetPaths.AddAssetPrev(path);
			}
			else
			{
				return loader;
			}

			AssetReference ar = null;
			if (m_ReferenceManager.TryGetAsset(path, out ar))
			{
#if ASSETMANAGER_LOG_ON
                Debug.Log("[AssetManage]CreateAssetAsyncLoader asset is loaded "+path+","+Time.frameCount);
#endif
				//refresh tag
				ar.AddTag(tag);

				loader = CreateAssetExistLoader(path);
				loader.result = ar;
				loader.autoReleaseBundle = autoReleaseBundle;
				//加载完成后由AssetManager释放loader
				loader.onAfterComplete += OnAssetAfterLoaded;
			}
			else
			{
				if (!m_AssetLoadings.TryGetValue(path, out loader))
				{
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]CreateAssetAsyncLoader create new loader " + path + "," + Time.frameCount);
#endif
					loader = CreateAssetAsyncLoader(path);
					m_AssetLoadings[path] = loader;

					//对加载前后做特殊处理。只要处理一次。
					loader.Init(OnAssetBeforeLoaded, OnAssetAfterLoaded);

					if (type != null)
					{
						loader.type = type;
					}
				}
				else
				{
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]CreateAssetAsyncLoader using loading loader " + path + "," + Time.frameCount);
#endif
					//资源的名子是唯一的。所以类型也要唯一。
					if (loader.type != type)
					{
						Debug.LogErrorFormat(
							"[AssetManage]CreateAssetAsyncLoader asset {0} is loading.But loading type={1} different with current type={2} ,{3}"
							, path, loader.type, type, +Time.frameCount);
					}
				}

				loader.AddParamTag(tag);

				loader.autoReleaseBundle = autoReleaseBundle;
			}

			return loader;
		}

		public void RemoveAssetLoading(AssetLoader loader)
		{
			AssetInfo info = loader.info;
			if (info != null)
			{
				//remove from loading
				if (m_AssetLoadings.ContainsKey(info.fullName))
				{
					m_AssetLoadings.Remove(info.fullName);
				}
			}
			else
			{
				string key = null;
				foreach (var iter in m_AssetLoadings)
				{
					if (iter.Value == loader)
					{
						key = iter.Key;
					}
				}

				if (!string.IsNullOrEmpty(key))
				{
					m_AssetLoadings.Remove(key);
				}
			}
		}

		public bool IsAssetLoading(string path)
		{
			return m_AssetLoadings.ContainsKey(path);
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

		public AssetBundleLoader LoadAssetBundleAsync(string path, int tag, bool cache,
			Action<AssetBundleReference> completeHandle = null,
			Action<AssetBundleLoader> beforLoadComplete = null,
			Action<AssetBundleLoader> afterLoadComplete = null)
		{
			AssetBundleLoader loader = CreateSingleAssetBundleAsyncLoader(path, tag, cache);

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


		public AssetBundleReference LoadAssetBundleSync(string path, int tag, bool cache = true)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			AssetBundleReference abr = null;

			if (m_ReferenceManager.TryGetAssetBundle(path, out abr))
			{
#if ASSETMANAGER_LOG_ON
                Debug.LogFormat("[AssetManage]LoadAssetBundleSync bundle is loaded {0},{1}", path, Time.frameCount);
#endif
				//refresh 
				abr.AddTag(tag);

				if (cache)
				{
					abr.Cache();
				}
			}
			else
			{
				if (IsAssetBundleLoading(path))
				{
					Debug.LogErrorFormat("[AssetManage]LoadAssetBundleSync async loader is active {0},{1}", path, Time.frameCount);
					//TODO Stop async
					return null;
				}
				else
				{
#if ASSETMANAGER_LOG_ON
                    Debug.LogFormat("[AssetManage]LoadAssetBundleSync create new loader {0},{1}", path, Time.frameCount);
#endif
					AssetBundleSyncLoader loader = CreateAssetBundleSyncLoader(path);
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


		public AssetBundleAsyncLoader CreateAssetBundleAsyncLoader(string path)
        {
			AssetBundleAsyncLoader loader = null;
            AssetBundleInfo info = null;
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            info = infoManager.FindAssetBundleInfo(path);
            if (info == null)
            {
                Debug.LogErrorFormat("[AssetManage]Can't find asset bundle info {0}", path);
                return null;
            }
            loader = LoaderPool.AssetBundleAsyncLoaderPool.Get();
#else
            loader = new AssetBundleEmptyLoader();
            //just for message
            info = new AssetBundleInfo();
            info.fullName = path;
#endif

            loader.info = info;
            loader.loaderManager = this;
            return loader;
        }

        public AssetBundleSyncLoader CreateAssetBundleSyncLoader(string path)
        {
            AssetBundleSyncLoader loader = null;

#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            AssetBundleInfo info = null;
            info = infoManager.FindAssetBundleInfo(path);
            if (info != null)
            {
                loader = LoaderPool.AssetBundleSyncLoaderPool.Get();
                loader.info = info;
                loader.loaderManager = this;
            }
            else
            {
                Debug.LogErrorFormat("[AssetManage]Can't find asset bundle info {0}", path);
            }
#endif

            return loader;
        }

		public AssetBundleAsyncLoader CreateAssetBundleExistLoader(string path)
		{
			AssetBundleAsyncExistLoader loader = LoaderPool.AssetBundleAsyncExistLoaderPool .Get();
			loader.loaderManager = this;
			return loader;
		}

		public AssetBundleAsyncLoader CreateSingleAssetBundleAsyncLoader(string path, int tag, bool cache)
		{
			AssetBundleAsyncLoader loader = null;

			if (string.IsNullOrEmpty(path))
			{
				return loader;
			}

			AssetBundleReference abr = null;
			if (m_ReferenceManager.TryGetAssetBundle(path, out abr))
			{
				//asset bundle is loaded
#if ASSETMANAGER_LOG_ON
                Debug.Log("[AssetManage]LoadAssetBundle asset bundle is loaded " + path + "," + Time.frameCount);
#endif
				//refresh tag
				abr.AddTag(tag);

				//cache abr
				if (cache)
				{
					abr.Cache();
				}

				//create call back loader
				loader = CreateAssetBundleExistLoader(path);
				loader.result = abr;

				loader.onAfterComplete += OnAssetBundleAfterLoaded;
			}
			else
			{
				if (!m_AssetBundleLoadings.TryGetValue(path, out loader))
				{
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]LoadAssetBundle create new loader " + path + "," + Time.frameCount);
#endif
					loader = CreateAssetBundleAsyncLoader(path);
					if (loader != null)
					{
						m_AssetBundleLoadings[path] = loader;
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
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]LoadAssetBundle using loading loader " + path + "," + Time.frameCount);
#endif
				}

				loader.AddParamTag(tag);

				loader.SetCacheResult(cache);
			}
			return loader;
		}

		public void RemoveAssetBundleLoading(AssetBundleLoader loader)
		{
			AssetBundleInfo info = loader.info;
			if (info != null)
			{
				if (m_AssetBundleLoadings.ContainsKey(info.fullName))
				{
					m_AssetBundleLoadings.Remove(info.fullName);
				}
			}
			else
			{
				string key = null;
				foreach (var iter in m_AssetBundleLoadings)
				{
					if (iter.Value == loader)
					{
						key = iter.Key;
					}
				}

				if (!string.IsNullOrEmpty(key))
				{
					m_AssetBundleLoadings.Remove(key);
				}
			}

		}

		public bool IsAssetBundleLoading(string path)
		{
			return m_AssetBundleLoadings.ContainsKey(path);
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
