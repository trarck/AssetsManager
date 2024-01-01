using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManage
{
    public class AssetManager : UnitySingleton<AssetManager>
    {
		IRequestManager m_RequestManager;
		IInfoManager m_InfoManager;
		IReferenceManager m_ReferenceManager;
		LoaderManager m_LoaderManager;


        bool m_Inited = false;

//#if UNITY_EDITOR
//		void Awake()
//        {
//            Init(AssetPaths.bundleManifestFile);
//        }
//#endif

		public void Init(string allManifestFile=null,Action<bool> callback=null)
        {
            if (m_Inited)
            {
                if (m_InfoManager.loading)
                {
                    m_InfoManager.onLoadComplete += callback;
                }
                else
                {
                    if (callback != null)
                    {
                        callback(true);
                    }
                }
                return;
            }
            m_Inited = true;
            
            //asset search path
            AssetPaths.SetupDefaultSearchPaths();

            //system events
            SetupSystemEvents();

			//create rquest manager
			m_RequestManager = new RequestManager();
			m_RequestManager.Init();

            //create info manager
            m_InfoManager = new InfoManager(this);
            m_InfoManager.Init();

			//create load result. asset reference manager
			m_ReferenceManager = new ReferenceManager();
			m_ReferenceManager.Init();

			//crate loader manager
			m_LoaderManager = new LoaderManager();
			m_LoaderManager.Init(m_InfoManager, m_RequestManager, m_ReferenceManager);

			//加载info文件
			if (callback != null)
			{
				m_InfoManager.onLoadComplete += callback;
			}

			if (string.IsNullOrEmpty(allManifestFile))
			{
				allManifestFile = AssetPaths.bundleManifestFile;
			}
			m_InfoManager.Load(AssetPaths.GetFullPath(allManifestFile));
		}

        /// <summary>
        /// 用于自定义构建
        /// </summary>
        /// <param name="infoManager"></param>
        /// <param name="loaderManager"></param>
        /// <param name="requestManager"></param>
        public void Init(IInfoManager infoManager, IRequestManager requestManager,IReferenceManager referenceManager, LoaderManager loaderManager)
        {
            if (m_Inited)
            {
                return;
            }

            m_Inited = true;

            SetupSystemEvents();

            m_InfoManager = infoManager;
            m_RequestManager = requestManager;
			m_ReferenceManager = referenceManager;
			m_LoaderManager = loaderManager;
		}


        public void Clean()
        {

			if (m_LoaderManager != null)
			{
				m_LoaderManager.Clean();
			}

			if (m_ReferenceManager != null)
			{
				m_ReferenceManager.Clean();
				m_ReferenceManager = null;
			}

			if (m_RequestManager != null)
			{
				m_RequestManager.Clean();
				m_RequestManager = null;
			}

			if (m_InfoManager != null)
			{
				m_InfoManager.Clean();
				m_InfoManager = null;
			}
        }

		public void Update()
		{
			if (m_RequestManager != null)
			{
				m_RequestManager.Update(Time.deltaTime);
			}
		}

		private void SetupSystemEvents()
		{
			Application.lowMemory += OnLowMemory;
		}

		private void OnLowMemory()
		{
			UnloadUnuseds();
		}

        #region load asset bundle


        /// <summary>
        /// 异步加载AssetBundle
        /// 不推荐手动加载AssetBundle，会通过加载Asset自动加载AssetBunde。
        /// </summary>
        /// <param name="bundleId">
        /// bundleId:bundle的唯一确定值。
        /// 可以是AssetBundle的路径hash，也可以是AssetBundle内容hash, 甚至可以是定义的序列值。
        /// 可以通过bundle中包含的资源，通过InfoManager获取。
        /// </param>
        /// <param name="cache">是否要缓存</param>
        /// <param name="completeHandle">加载完成回调</param>
        /// <returns>loader</returns>
        public AssetBundleLoader LoadAssetBundle(ulong bundleId, bool cache, Action<AssetBundleReference> completeHandle = null)
        {
            return LoadAssetBundle(bundleId, 0, cache, completeHandle);
        }

		/// <summary>
		/// async load asset bundle
		/// 同一个资源只有一个正在加载的loader
		/// </summary>
		/// <param name="bundleId">asset bundle id</param>
		/// <param name="tag">tag for loaded asset bundle</param>
		/// <param name="cache">cache asset bundle</param>
		/// <param name="completeHandle">load complete callback</param>
		/// <param name="beforLoadComplete">before load complete callback.use for custom loader</param>
		/// <param name="afterLoadComplete">after load complete callback.use for custom loader</param>
		/// <returns></returns>
		public AssetBundleLoader LoadAssetBundle(ulong bundleId, int tag, bool cache,
			Action<AssetBundleReference> completeHandle = null,
			Action<AssetBundleLoader> beforLoadComplete = null,
			Action<AssetBundleLoader> afterLoadComplete = null)
		{
			return m_LoaderManager.LoadAssetBundleAsync(bundleId,tag,cache,completeHandle,beforLoadComplete,afterLoadComplete);
		}

		/// <summary>
		/// 同步加载AssetBundle
		/// </summary>
		/// <param name="bundleId"></param>
		/// <param name="cache"></param>
		/// <returns></returns>
        public AssetBundleReference LoadAssetBundleSync(ulong bundleId, bool cache = true)
        {
            return LoadAssetBundleSync(bundleId, 0, cache);
        }

        /// <summary>
        /// load asset bundle from file
        /// </summary>
        /// <param name="bundleId"></param>
        /// <param name="tag"></param>
        /// <param name="cache"></param>
        /// <returns>AssetBundleReference retainted.ref count add one after load.</returns>
        public AssetBundleReference LoadAssetBundleSync(ulong bundleId, int tag, bool cache = true)
        {
            return m_LoaderManager. LoadAssetBundleSync(bundleId,tag,cache);
        }
		#endregion

		#region load asset path string

		public AssetLoaderOperation LoadAsset(string path, Action<AssetReference> completeHandle=null, bool autoReleaseBundle = true)
        {
            return LoadAsset(path, 0,null, autoReleaseBundle, completeHandle);
        }

        public AssetLoaderOperation LoadAsset<T>(string path, Action<AssetReference> completeHandle=null, bool autoReleaseBundle = true)
        {
            return LoadAsset(path, 0,  typeof(T), autoReleaseBundle, completeHandle);
        }

		public AssetLoaderOperation LoadAsset<T>(string path, int tag, Type type,Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
		{
			return LoadAsset(path, tag, typeof(T), autoReleaseBundle, completeHandle);
		}
		/// <summary>
		/// 资源加载
		/// 资源加载完成，返回一个关于资源的refrence，记录资源的使用情况。
		/// 资源使用的三种方式：
		///     1.Retain(),使用完成时需要执行Release()。
		///     2.Retain(Object),使用完成时可以不用执行Release(Object)，等待UnloadUnuseds清理。
		///     3.Monitor(GameObject),当GameObject被删除时，会自动执行Release(Object)。
		/// 对于手动删除资源最好执行RemoveAsset。
		/// 同一个资源只有一个正在加载的loader。由Manager负责管理Loader。
		/// </summary>
		/// <param name="path"></param>
		/// <param name="tag"></param>
		/// <param name="type"></param>
		/// <param name="completeHandle"></param>
		/// <returns></returns>
		public AssetLoaderOperation LoadAsset(string path,int tag, Type type,bool autoReleaseBundle,
			Action<AssetReference> completeHandle=null, 
			Action<AssetLoader> beforLoadComplete = null,
			Action<AssetLoader> afterLoadComplete = null)
        {
            return m_LoaderManager.LoadAssetAsync(path, tag,type,autoReleaseBundle,completeHandle,beforLoadComplete,afterLoadComplete);
        }

		public AssetLoaderOperation LoadAssetWithAlias(string alias, int tag, Type type, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
		{
			AssetLoadInfo assetInfo = m_InfoManager.GetAssetInfoWithAlias(alias);
			if (assetInfo != null)
			{
				return LoadAsset(assetInfo.path, tag, type, autoReleaseBundle, completeHandle);
			}
			else
			{
				AMDebug.LogErrorFormat("[AssetManage]LoadAsset no alias {0} find ", alias);
			}
			return AssetLoaderOperation.Empty;
		}

        public AssetReference LoadAssetSync(string path)
        {
            return LoadAssetSync(path, 0, null);
        }

        public AssetReference LoadAssetSync(string path, int tag)
        {
            return LoadAssetSync(path, tag, null);
        }

        public AssetReference LoadAssetSync(string path, int tag, Type type)
        {
			return m_LoaderManager.LoadAssetSync(path, tag, type);
        }

        public void LoadAssets(ICollection<string> assets, Action<Dictionary<string, AssetReference>> callback)
        {
            Dictionary<string, AssetReference> assetReferences = new Dictionary<string, AssetReference>();
            LoadAssets(assets, assetReferences, callback);
        }

        public void LoadAssets(ICollection<string> assets, Dictionary<string, AssetReference> assetReferences, Action<Dictionary<string, AssetReference>> callback)
        {
            int needCount = assets.Count;
            int loadCount = 0;
            bool checkAll = false;

            foreach (var asset in assets)
            {
                if (!string.IsNullOrEmpty(asset))
                {
                    ++loadCount;
                    AssetManager.Instance.LoadAsset(asset, (assetReference) => {
                        if (assetReference != null && !assetReference.IsEmpty())
                        {
                            if (assetReferences != null)
                            {
                                assetReferences[asset] = assetReference;
                            }
                        }
                        else
                        {
                            AMDebug.LogErrorFormat("[AssetManage]LoadAssets can't load {0}", asset);
                        }

                        --needCount;

                        if (--loadCount <= 0)
                        {
                            if (checkAll)
                            {
                                //all finished
                                if (callback != null)
                                {
                                    callback(assetReferences);
                                }
                            }
                        }
                    });
                }
                else
                {
                    --needCount;
                }
            }

            checkAll = true;

            if (needCount == 0 && loadCount<=0)
            {
                if (callback != null)
                {
                    callback(assetReferences);
                }
            }
        }
        #endregion

        #region load asset path hash

        public AssetLoaderOperation LoadAsset(ulong path, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            return LoadAsset(path, 0, null, autoReleaseBundle, completeHandle);
        }

        public AssetLoaderOperation LoadAsset<T>(ulong path, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            return LoadAsset(path, 0, typeof(T), autoReleaseBundle, completeHandle);
        }

        public AssetLoaderOperation LoadAsset<T>(ulong path, int tag, Type type, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            return LoadAsset(path, tag, typeof(T), autoReleaseBundle, completeHandle);
        }
        /// <summary>
        /// 资源加载
        /// 资源加载完成，返回一个关于资源的refrence，记录资源的使用情况。
        /// 资源使用的三种方式：
        ///     1.Retain(),使用完成时需要执行Release()。
        ///     2.Retain(Object),使用完成时可以不用执行Release(Object)，等待UnloadUnuseds清理。
        ///     3.Monitor(GameObject),当GameObject被删除时，会自动执行Release(Object)。
        /// 对于手动删除资源最好执行RemoveAsset。
        /// 同一个资源只有一个正在加载的loader。由Manager负责管理Loader。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="type"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetLoaderOperation LoadAsset(ulong path, int tag, Type type, bool autoReleaseBundle,
            Action<AssetReference> completeHandle = null,
            Action<AssetLoader> beforLoadComplete = null,
            Action<AssetLoader> afterLoadComplete = null)
        {
            return m_LoaderManager.LoadAssetAsync(path, tag, type, autoReleaseBundle, completeHandle, beforLoadComplete, afterLoadComplete);
        }

        public AssetReference LoadAssetSync(ulong path)
        {
            return LoadAssetSync(path, 0, null);
        }

        public AssetReference LoadAssetSync(ulong path, int tag)
        {
            return LoadAssetSync(path, tag, null);
        }

        public AssetReference LoadAssetSync(ulong path, int tag, Type type)
        {
            return m_LoaderManager.LoadAssetSync(path, tag, type);
        }

        public void LoadAssets(ICollection<ulong> assets, Action<Dictionary<ulong, AssetReference>> callback)
        {
            Dictionary<ulong, AssetReference> assetReferences = new Dictionary<ulong, AssetReference>();
            LoadAssets(assets, assetReferences, callback);
        }

        public void LoadAssets(ICollection<ulong> assets, Dictionary<ulong, AssetReference> assetReferences, Action<Dictionary<ulong, AssetReference>> callback)
        {
            int needCount = assets.Count;
            int loadCount = 0;
            bool checkAll = false;

            foreach (var assetPathHash in assets)
            {
                if (assetPathHash>0)
                {
                    ++loadCount;
                    AssetManager.Instance.LoadAsset(assetPathHash, (assetReference) => {
                        if (assetReference != null && !assetReference.IsEmpty())
                        {
                            if (assetReferences != null)
                            {
                                assetReferences[assetPathHash] = assetReference;
                            }
                        }
                        else
                        {
                            AMDebug.LogErrorFormat("[AssetManage]LoadAssets can't load {0}", assetPathHash);
                        }

                        --needCount;

                        if (--loadCount <= 0)
                        {
                            if (checkAll)
                            {
                                //all finished
                                if (callback != null)
                                {
                                    callback(assetReferences);
                                }
                            }
                        }
                    });
                }
                else
                {
                    --needCount;
                }
            }

            checkAll = true;

            if (needCount == 0 && loadCount <= 0)
            {
                if (callback != null)
                {
                    callback(assetReferences);
                }
            }
        }
        #endregion

        #region Yield Load Asset Bundle
        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cacheLoadedAsset"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public BundleLoaderEnumerator YieldLoadAssetBundle(ulong bundleId, bool cacheLoadedAsset)
        {
            return YieldLoadAssetBundle(bundleId, 0, cacheLoadedAsset);
        }

        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="cacheLoadedAsset"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public BundleLoaderEnumerator YieldLoadAssetBundle(ulong bundleId, int tag, bool cacheLoadedAsset)
        {
            BundleLoaderEnumerator bundleLoaderEnumerator = new BundleLoaderEnumerator();
            LoadAssetBundle(bundleId, tag, cacheLoadedAsset, bundleLoaderEnumerator.OnAssetBundleLoadComlete);
            return bundleLoaderEnumerator;
        }

#endregion

#region Yield Load Asset
        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <param name="path"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetLoaderEnumerator YieldLoadAsset(string path)
        {
            return YieldLoadAsset(path, 0, null);
        }

        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetLoaderEnumerator YieldLoadAsset<T>(string path)
        {
            return YieldLoadAsset(path, 0, typeof(T));
        }

        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetLoaderEnumerator YieldLoadAsset<T>(string path, string tag)
        {
            return YieldLoadAsset(path, 0, typeof(T));
        }

		/// <summary>
		/// 使用yield要注意loader的释放。使用using或手动调用dispose
		/// </summary>
		/// <param name="path"></param>
		/// <param name="tag"></param>
		/// <param name="type"></param>
		/// <param name="completeHandle"></param>
		/// <returns></returns>
		public AssetLoaderEnumerator YieldLoadAsset(string path, int tag, Type type, bool autoReleaseBundle = true)
		{
			AssetLoaderEnumerator assetLoaderEnumerator = new AssetLoaderEnumerator();
			LoadAsset(path, tag, type, autoReleaseBundle, assetLoaderEnumerator.OnAssetLoadComlete);
			return assetLoaderEnumerator;
		}

#endregion


#region Load Scene
        public AssetBundleLoader LoadScene(string path, int tag, Action<AssetBundleReference> completeHandle)
        {
            AssetLoadInfo info = m_InfoManager.GetAssetInfo(path);
            if (info != null)
            {
                return LoadAssetBundle(info.bunldeId, tag, false, completeHandle);
            }
            else
            {
                ulong bundleId = xxHash.xxHash64.ComputeHash(path);
                return LoadAssetBundle(bundleId, tag, false, completeHandle);
            }
        }

#endregion
		  		   
#region Asset Ref 
        public void UnloadUnuseds()
        {
			m_ReferenceManager.UnloadUnuseds();
        }

        public void UnloadUnuseds(int tag,bool removeTag=true)
        {
			m_ReferenceManager.UnloadUnuseds(tag, removeTag);
        }


        public void UncacheAll()
        {
			m_ReferenceManager.UncacheAll();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleId"></param>
        public void UncacheAssetBundle(ulong bundleId)
        {
			m_ReferenceManager.UncacheAssetBundle(bundleId);
		}

        public void UncacheAssetBundle(AssetBundleReference abr)
        {
			m_ReferenceManager.UncacheAssetBundle(abr);
		}

        public void UncacheAsset(string assetPath)
        {
            if (!string.IsNullOrEmpty(assetPath))
            {
                UncacheAsset(xxHash.xxHash64.ComputeHash(assetPath));
            }
        }

        public void UncacheAsset(ulong assetPathHash)
        {
            m_ReferenceManager.UncacheAsset(assetPathHash);
        }

        public void UncacheAsset(AssetReference ar)
        {
			m_ReferenceManager.UncacheAsset(ar);
		}

        public void RemoveTags(int tag)
        {
			m_ReferenceManager.RemoveTags(tag);

		}

        /// <summary>
        /// remove asset's asset bundle reference
        /// </summary>
        public void BreakAssetsBundleReferenceAll()
        {
			m_ReferenceManager.BreakAssetsBundleReferenceAll();
        }

        /// <summary>
        /// remove asset's asset bundle reference
        /// when asset is loaded from asset bundle,normally asset bundle can be unload(false).
        /// this can be reduce memory used.
        /// 如果asset bunlde被多个asset引用，而同时加载这些asset的时候，不能一个一个删除，可以在加载完成后集中删除。
        /// </summary>
        /// <param name="tag"></param>
        public void BreakAssetBundleReferenceByTag(int tag)
        {
			m_ReferenceManager.BreakAssetBundleReferenceByTag(tag);
		}

		#endregion

		#region loaded manage
		//void OnAssetBundleBeforeLoaded(AssetBundleLoader loader)
  //      {
  //          AssetBundleReference abr = loader.result;
		//	m_ReferenceManager.AddAssetBundleReference(abr, loader.cacheResult);

  //          AssetBundleInfo info = loader.info;
  //          if (info != null)
  //          {
  //              if (m_AssetBundleLoadings.ContainsKey(info.fullName))
  //              {
  //                  m_AssetBundleLoadings.Remove(info.fullName);
  //              }
  //          }
  //          else
  //          {
  //              string key = null;
  //              foreach (var iter in m_AssetBundleLoadings)
  //              {
  //                  if (iter.Value == loader)
  //                  {
  //                      key = iter.Key;
  //                  }
  //              }

  //              if (!string.IsNullOrEmpty(key))
  //              {
  //                  m_AssetBundleLoadings.Remove(key);
  //              }
  //          }
  //      }

  //      void OnAssetBundleAfterLoaded(AssetBundleLoader loader)
  //      {
  //          m_LoaderManager.ReleaseLoader(loader);
  //      }

   //     void OnAssetBeforeLoaded(AssetLoader loader)
   //     {
   //         AssetReference ar = loader.result;
			//m_ReferenceManager.AddAssetReference(ar);

			////remove from loading
			//AssetInfo info = loader.info;
			//if (info!=null)
   //         {
   //             //remove from loading
   //             if (m_AssetLoadings.ContainsKey(info.fullName))
   //             {
   //                 m_AssetLoadings.Remove(info.fullName);
   //             }
   //         }
   //         else
   //         {
   //             string key = null;
   //             foreach(var iter in m_AssetLoadings)
   //             {
   //                 if (iter.Value == loader)
   //                 {
   //                     key = iter.Key;
   //                 }
   //             }

   //             if (!string.IsNullOrEmpty(key))
   //             {
   //                 m_AssetLoadings.Remove(key);
   //             }
   //         }
   //     }

   //     void OnAssetAfterLoaded(AssetLoader loader)
   //     {
   //         if (loader.autoReleaseBundle)
   //         {
   //             AssetReference ar = loader.result;
   //             if (ar != null)
   //             {
   //                 ar.ReleaseBundleReference();
   //             }
   //         }

   //         m_LoaderManager.ReleaseLoader(loader);
   //     }

#endregion

        public IInfoManager infoManager
        {
            get { return m_InfoManager; }
            set {m_InfoManager = value;}
        }

        public IRequestManager requestManager
        {
            get { return m_RequestManager; }
            set { m_RequestManager = value; }
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

		public LoaderManager loaderManager
		{
			get
			{
				return m_LoaderManager;
			}
			set
			{
				m_LoaderManager = value;
			}
		}
		public Dictionary<ulong, AssetBundleReference> assetBundles
        {
            get
            {
                return m_ReferenceManager!=null? m_ReferenceManager.assetBundles:null;
            }
        }

        public Dictionary<ulong, AssetReference> assets
        {
            get
            {
				return m_ReferenceManager != null ? m_ReferenceManager.assets : null;
            }
        }
    }
}
