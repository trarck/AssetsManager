using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManage
{
    public class AssetManager : UnitySingleton<AssetManager>
    {
        //all loaded  asset bundles
        Dictionary<string, AssetBundleReference> m_AssetBundles = new Dictionary<string, AssetBundleReference>();

        //all loaded  asset bundles.usefull preload
        Dictionary<string, AssetReference> m_Assets = new Dictionary<string, AssetReference>();

        //loading loaders
        Dictionary<string, AssetBundleLoader> m_LoadingAssetBundleLoaders = new Dictionary<string, AssetBundleLoader>();
        Dictionary<string, AssetLoader> m_LoadingAssetLoaders = new Dictionary<string, AssetLoader>();

        IInfoManager m_InfoManager;
        LoaderManager m_LoaderManager;
        IRequestManager m_RequestManager;

        bool m_Inited = false;
#if UNITY_EDITOR
        void Awake()
        {
            Init(AssetPaths.bundleManifestFile);
        }
#endif

        #region Init

        protected void SetupSystemEvents()
        {
            Application.lowMemory += OnLowMemory;
        }

        public void Init(string allManifestFile=null,Action<bool> callback=null)
        {
            if (m_Inited)
            {
                if (m_InfoManager.inited)
                {
                    if (callback != null)
                    {
                        callback(true);
                    }
                }
                else
                {
                    m_InfoManager.onInitComplete += callback;
                }
                return;
            }
            m_Inited = true;
            
            //asset search path
            AssetPaths.SetupDefaultSearchPaths();

            //system events
            SetupSystemEvents();

            //crate loader manager
            m_LoaderManager = new LoaderManager(this);

            //create rquest manager
            m_RequestManager = new RequestManager(this);
            m_RequestManager.Init();

            //create info manager
            m_InfoManager = new InfoManager(this);
            m_InfoManager.Init();
            if (callback != null)
            {
                m_InfoManager.onInitComplete += callback;
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
        public void Init(IInfoManager infoManager,LoaderManager loaderManager,IRequestManager requestManager)
        {
            if (m_Inited)
            {
                return;
            }

            m_Inited = true;

            SetupSystemEvents();

            m_InfoManager = infoManager;
            m_LoaderManager = loaderManager;
            m_RequestManager = requestManager;
        }

#endregion

        public void Clean()
        {
            m_AssetBundles.Clear();
            m_Assets.Clear();
            m_LoadingAssetBundleLoaders.Clear();
            m_LoadingAssetLoaders.Clear();
        }

#region load asset bundle

        public AssetBundleLoader LoadAssetBundle(string path, bool cacheLoadedAsset, Action<AssetBundleReference> completeHandle=null)
        {
            return LoadAssetBundle(path,0, cacheLoadedAsset, completeHandle);
        }

        /// <summary>
        /// async load asset bundle
        /// 同一个资源只有一个正在加载的loader
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="cacheLoadedAsset"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetBundleLoader LoadAssetBundle(string path,int tag,bool cacheLoadedAsset, Action<AssetBundleReference> completeHandle=null,bool autoStart=true)
        {
            AssetBundleLoader loader = null;

            if (string.IsNullOrEmpty(path))
            {
                if (completeHandle != null)
                {
                    completeHandle(null);
                }
                return loader;
            }

            if (m_AssetBundles.ContainsKey(path))
            {
#if ASSETMANAGER_LOG_ON
                Debug.Log("[AssetManage]LoadAssetBundle asset bundle is loaded " + path + "," + Time.frameCount);
#endif
                //asset bundle is loaded
                AssetBundleReference abr = m_AssetBundles[path];

                //refresh 
                abr.AddTag(tag);

                if (cacheLoadedAsset)
                {
                    abr.Cache();
                }

                loader = m_LoaderManager.CreateAssetBundleAsyncLoader(path);
                loader.forceDone = true;
                loader.result = abr;

                //call complete callback
                if (completeHandle != null)
                {
                    loader.onComplete += completeHandle;
                }

                loader.IncreaseLoadingRequest();

                loader.onAfterComplete += OnAssetBundleAfterLoaded;
                loader.state = Loader.State.Completed;
                if (autoStart)
                {
                    m_LoaderManager.ActiveLoader(loader);
                }
            }
            else
            {
                if (m_LoadingAssetBundleLoaders.ContainsKey(path))
                {
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]LoadAssetBundle using loading loader " + path + "," + Time.frameCount);
#endif
                    loader = m_LoadingAssetBundleLoaders[path];
                }
                else
                {
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]LoadAssetBundle create new loader " + path + "," + Time.frameCount);
#endif
                    loader = m_LoaderManager.CreateAssetBundleAsyncLoader(path);
                    if (loader!=null)
                    {
                        m_LoadingAssetBundleLoaders[path] = loader;
                    }
                    else
                    {
                        if (completeHandle != null)
                        {
                            completeHandle(null);
                        }
                        return null;
                    }
                }
                
                loader.AddParamTag(tag);

                if(loader.cacheLoadedAsset==false && cacheLoadedAsset)
                {
                    loader.cacheLoadedAsset = true;
                }

                if (completeHandle != null)
                {
                    loader.onComplete += completeHandle;
                }

                if (loader.state == Loader.State.Idle)
                {
                    loader.onBeforeComplete += OnAssetBundleBeforeLoaded;
                    loader.onAfterComplete += OnAssetBundleAfterLoaded;
                    loader.state = Loader.State.Inited;
                    if (autoStart)
                    {
                        m_LoaderManager.ActiveLoader(loader);
                    }
                }
            }
            return loader;
        }

        public AssetBundleReference LoadAssetBundleSync(string path, bool cacheLoadedAsset = true)
        {
            return LoadAssetBundleSync(path, 0, cacheLoadedAsset);
        }

        /// <summary>
        /// load asset bundle from file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="cacheLoadedAsset"></param>
        /// <returns>AssetBundleReference retainted.ref count add one after load.</returns>
        public AssetBundleReference LoadAssetBundleSync(string path, int tag, bool cacheLoadedAsset = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            AssetBundleReference abr = null;

            if (m_AssetBundles.ContainsKey(path))
            {
#if ASSETMANAGER_LOG_ON
                Debug.LogFormat("[AssetManage]LoadAssetBundleSync bundle is loaded {0},{1}", path, Time.frameCount);
#endif
                abr = m_AssetBundles[path];
                //refresh 
                abr.AddTag(tag);

                if (cacheLoadedAsset)
                {
                    abr.Cache();
                }
            }
            else
            {
                if (m_LoadingAssetBundleLoaders.ContainsKey(path))
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
                    AssetBundleSyncLoader loader = m_LoaderManager.CreateAssetBundleSyncLoader(path);
                    if (loader != null)
                    {
                        loader.state = Loader.State.Inited;
                        if (loader.cacheLoadedAsset == false && cacheLoadedAsset)
                        {
                            loader.cacheLoadedAsset = true;
                        }

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
#endregion

#region load asset
        public AssetLoader LoadAsset(string path, Action<AssetReference> completeHandle=null, bool autoReleaseBundle = true)
        {
            return LoadAsset(path, 0,null, completeHandle, autoReleaseBundle);
        }

        public AssetLoader LoadAsset<T>(string path, Action<AssetReference> completeHandle=null, bool autoReleaseBundle = true)
        {
            return LoadAsset(path, 0,  typeof(T), completeHandle, autoReleaseBundle);
        }

        public AssetLoader LoadAsset<T>(string path, string tag,Action<AssetReference> completeHandle=null, bool autoReleaseBundle = true)
        {
            return LoadAsset(path, 0,  typeof(T), completeHandle, autoReleaseBundle);
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
        public AssetLoader LoadAsset(string path,int tag, Type type,Action<AssetReference> completeHandle=null,bool autoReleaseBundle=true,bool autoStart=true)
        {
            AssetLoader loader = null;
            if (!string.IsNullOrEmpty(path))
            {
                path = AssetPaths.AddAssetPrev(path);
            }
            else
            {
                if (completeHandle != null)
                {
                    completeHandle(null);
                }
                return loader;
            }


            if (m_Assets.ContainsKey(path))
            {
#if ASSETMANAGER_LOG_ON
                Debug.Log("[AssetManage]LoadAsset asset is loaded "+path+","+Time.frameCount);
#endif
                AssetReference ar = m_Assets[path];

                //refresh
                ar.AddTag(tag);

                //cache asset
                ar.Cache();

                loader = m_LoaderManager.CreateAssetAsyncLoader(path);
                loader.forceDone = true;
                loader.result = ar;

                if (completeHandle != null)
                {
                    loader.onComplete += completeHandle;
                }

                loader.onAfterComplete += OnAssetAfterLoaded;
                loader.state = Loader.State.Completed;
                if (autoStart)
                {
                    m_LoaderManager.ActiveLoader(loader);
                }
            }
            else
            {
                if (m_LoadingAssetLoaders.ContainsKey(path))
                {
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]LoadAsset using loading loader " + path + "," + Time.frameCount);
#endif
                    loader = m_LoadingAssetLoaders[path];
                }
                else
                {
#if ASSETMANAGER_LOG_ON
                    Debug.Log("[AssetManage]LoadAsset create new loader " + path + "," + Time.frameCount);
#endif
                    loader = m_LoaderManager.CreateAssetAsyncLoader(path);
                    m_LoadingAssetLoaders[path] = loader;
                }
                
                loader.AddParamTag(tag);

                if (type != null)
                {
                    loader.type = type;
                }

                if (!autoReleaseBundle)
                {
                    loader.autoReleaseBundle = autoReleaseBundle;
                }

                if (completeHandle != null)
                {
                    loader.onComplete += completeHandle;
                }
                
                loader.IncreaseLoadingRequest();

                //only once init
                if (loader.state == Loader.State.Idle)
                {
                    loader.onBeforeComplete += OnAssetBeforeLoaded;
                    loader.onAfterComplete += OnAssetAfterLoaded;
                    loader.state = Loader.State.Inited;
                    if (autoStart)
                    {
                        m_LoaderManager.ActiveLoader(loader);
                    }
                }
            }

            return loader;
        }

        public AssetLoader LoadAssetWithAlias(string alias, int tag, Type type, Action<AssetReference> completeHandle=null)
        {
            AssetInfo assetInfo = m_InfoManager.FindAssetInfoWithAlias(alias);
            if (assetInfo != null)
            {
                return LoadAsset(assetInfo.fullName, tag, type, completeHandle);
            }
            else
            {
                Debug.LogErrorFormat("[AssetManage]LoadAsset no alias {0} find ", alias);
            }
            return null;
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
            if (!string.IsNullOrEmpty(path))
            {
                path = AssetPaths.AddAssetPrev(path);
            }

            AssetReference ar = null;

            AssetLoader loader = null;

            if (m_Assets.ContainsKey(path))
            {
#if ASSETMANAGER_LOG_ON
                Debug.Log("[AssetManage]LoadAssetSync asset is loaded " + path + "," + Time.frameCount);
#endif
                ar = m_Assets[path];

                //refresh
                ar.AddTag(tag);

                //cache asset
                ar.Cache();
            }
            else
            {
                if (m_LoadingAssetLoaders.ContainsKey(path))
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
                    loader = m_LoaderManager.CreateAssetSyncLoader(path);
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
                            Debug.LogErrorFormat("[AssetManage]LoadAssets can't load {0}", asset);
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

#region Yield Load Asset Bundle
        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cacheLoadedAsset"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public BundleLoaderEnumerator YieldLoadAssetBundle(string path, bool cacheLoadedAsset)
        {
            return YieldLoadAssetBundle(path, 0, cacheLoadedAsset);
        }

        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="cacheLoadedAsset"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public BundleLoaderEnumerator YieldLoadAssetBundle(string path, int tag, bool cacheLoadedAsset)
        {
            BundleLoaderEnumerator bundleLoaderEnumerator = new BundleLoaderEnumerator();
            LoadAssetBundle(path, tag, cacheLoadedAsset, bundleLoaderEnumerator.OnAssetBundleLoadComlete);
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
        public AssetLoaderEnumerator YieldLoadAsset(string path, int tag, Type type)
        {
            AssetLoaderEnumerator assetLoaderEnumerator = new AssetLoaderEnumerator();
            LoadAsset(path, tag, type, assetLoaderEnumerator.OnAssetLoadComlete);
            return assetLoaderEnumerator;
        }

#endregion

        void Update()
        {
            if (m_RequestManager!=null)
            {
                m_RequestManager.Update(Time.deltaTime);
            }
        }

#region exter function
        public AssetBundleLoader LoadScene(string path, int tag, Action<AssetBundleReference> completeHandle)
        {
            AssetInfo info = m_InfoManager.FindAssetInfo(path);
            if (info != null)
            {
                return LoadAssetBundle(info.bundleName, tag, false, completeHandle);
            }
            else
            {
                return LoadAssetBundle(path, tag, false, completeHandle);
            }
        }

#endregion

        void OnLowMemory()
        {
            UnloadUnuseds();
        }

#region unload unused
        public void UnloadUnuseds()
        {
            UnloadUnusedAssets();
            UnloadUnusedBundles();
            Resources.UnloadUnusedAssets();
        }

        public void UnloadUnuseds(int tag,bool removeTag=true)
        {
            UnloadUnusedAssets(tag);
            UnloadUnusedBundles(tag);
            if (removeTag)
            {
                RemoveTags(tag);
            }
            Resources.UnloadUnusedAssets();
        }

        public void UnloadUnusedBundles()
        {
            if (m_AssetBundles.Count == 0)
            {
                return;
            }

            AssetBundleReference abr=null;

            Stack<string> checkQueue = StackPool<string>.Get();
            HashSet<string> checkings = HashSetPool<string>.Get();

            foreach(string key in m_AssetBundles.Keys)
            {
                abr = m_AssetBundles[key];
                if (abr.isCache)
                {
                    checkQueue.Push(key);
                    checkings.Add(key);
                }
            }

            Action<string> checkFun = (key) =>
            {
                abr = m_AssetBundles[key];
                checkings.Remove(key);

                if (abr.isUnused())
                {
                    //check dependencies
                    if (abr.dependencies != null && abr.dependencies.Count > 0)
                    {
                        foreach (AssetBundleReference sub in abr.dependencies)
                        {
                            if (sub.isCache && !checkings.Contains(sub.name))
                            {
                                checkQueue.Push(sub.name);
                            }
                        }
                    }

                    abr.Dispose();
                    m_AssetBundles.Remove(key);
                }
            };

            //recheck unused asset bundle
            while(checkQueue.Count>0){
                checkFun(checkQueue.Pop());
            }

            StackPool<string>.Release(checkQueue);
            HashSetPool<string>.Release(checkings);
        }

        public void UnloadUnusedBundles(int tag)
        {
            if (m_AssetBundles.Count == 0)
            {
                return;
            }

            AssetBundleReference abr = null;

            Stack<string> checkQueue = StackPool<string>.Get();
            HashSet<string> checkings = HashSetPool<string>.Get();


            Action<string> checkFun = (key) =>
            {
                abr = m_AssetBundles[key];
                checkings.Remove(key);

                if (abr.isUnused())
                {
                    //check dependencies
                    if (abr.dependencies != null && abr.dependencies.Count > 0)
                    {
                        foreach (AssetBundleReference sub in abr.dependencies)
                        {
                            //只有同样tag和空tag的ref才需要重新检查。
                            if (sub.isCache && (sub.tagCount == 0 || sub.HaveTag(tag)) && !checkings.Contains(sub.name))
                            {
                                checkQueue.Push(sub.name);
                            }
                        }
                    }

                    abr.Dispose();
                    m_AssetBundles.Remove(key);
                }
            };

            foreach (string key in m_AssetBundles.Keys)
            {
                abr = m_AssetBundles[key];
                if (abr.HaveTag(tag) && abr.isCache)
                {
                    checkQueue.Push(key);
                    checkings.Add(key);
                }
            }

            //recheck unused asset bundle
            while (checkQueue.Count > 0)
            {
                checkFun(checkQueue.Pop());
            }

            StackPool<string>.Release(checkQueue);
            HashSetPool<string>.Release(checkings);            
        }

        public void UnloadUnusedAssets()
        {
            if (m_Assets.Count == 0)
            {
                return;
            }
            AssetReference ar = null;
            List<string> keys = ListPool<string>.Get();
            keys.AddRange(m_Assets.Keys);

            for (int i = 0, l = keys.Count; i < l; ++i)
            {
                ar = m_Assets[keys[i]];
                if (ar.isUnused())
                {
                    ar.Dispose();
                    m_Assets.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }

        public void UnloadUnusedAssets(int tag)
        {
            if (m_Assets.Count == 0)
            {
                return;
            }

            AssetReference ar = null;
            List<string> keys = ListPool<string>.Get();
            keys.AddRange(m_Assets.Keys);

            for (int i = 0, l = keys.Count; i < l; ++i)
            {
                ar = m_Assets[keys[i]];
                if (ar.HaveTag(tag))
                {
                    if (ar.isUnused())
                    {
                        ar.Dispose();
                        m_Assets.Remove(keys[i]);
                    }
                }
            }
            ListPool<string>.Release(keys);
        }

#endregion

#region unused
        void UnusedAllAssetBundles()
        {
            foreach(var iter in m_AssetBundles)
            {
                AssetBundleReference abr = iter.Value;
                abr.UnCache();
            }
        }

        void UnusedAllAssets()
        {
            foreach(var iter in m_Assets)
            {
                AssetReference ar = iter.Value;
                ar.UnCache();
            }
        }

        public void UnusedAll()
        {
            UnusedAllAssets();
            UnusedAllAssetBundles();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetBundleName"></param>
        public void UnusedAssetBundle(string assetBundleName)
        {
            if (m_AssetBundles.ContainsKey(assetBundleName))
            {
                AssetBundleReference abr = m_AssetBundles[assetBundleName];
                abr.UnCache();
            }
        }

        public void UnusedAssetBundle(AssetBundleReference abr)
        {
            if (abr != null)
            {
                abr.UnCache();
            }
        }

        public void UnusedAsset(string assetName)
        {
            if (m_Assets.ContainsKey(assetName))
            {
                AssetReference ar = m_Assets[assetName];
                ar.UnCache();
            }
        }

        public void UnusedAsset(AssetReference ar)
        {
            if (ar != null)
            {
                ar.UnCache();
            }
        }

#endregion

        //public void RemoveAssetBundle(string assetBundleName)
        //{
        //    if (m_AssetBundles.ContainsKey(assetBundleName))
        //    {
        //        AssetBundleReference abr = m_AssetBundles[assetBundleName];
        //        m_AssetBundles.Remove(assetBundleName);
        //        abr.onDispose -= OnAssetBundleDispose;
        //        abr.Dispose();
        //    }
        //}

        //public void RemoveAssetBundle(AssetBundleReference abr)
        //{
        //    if (m_AssetBundles.ContainsKey(abr.name))
        //    {
        //        m_AssetBundles.Remove(abr.name);
        //        abr.onDispose -= OnAssetBundleDispose;
        //        abr.Dispose();
        //    }
        //}

        //public void RemoveAsset(string assetName)
        //{
        //    if (m_Assets.ContainsKey(assetName))
        //    {
        //        AssetReference ar = m_Assets[assetName];
        //        m_Assets.Remove(assetName);
        //        ar.onDispose -= OnAssetDispose;
        //        ar.Dispose();
        //    }
        //}

        //public void RemoveAsset(AssetReference ar)
        //{
        //    if (m_Assets.ContainsValue(ar))
        //    {
        //        m_Assets.Remove(ar.name);
        //        ar.onDispose -= OnAssetDispose;
        //        ar.Dispose();
        //    }
        //}

#region remove tags
        public void RemoveTags(int tag)
        {
            RemoveAssetsTag(tag);
            RemoveAssetBundlesTag(tag);
        }

        protected void RemoveAssetBundlesTag(int tag)
        {
            if (m_AssetBundles.Count == 0)
            {
                return;
            }

            AssetBundleReference abr = null;
            var iter = m_AssetBundles.GetEnumerator();
            while (iter.MoveNext())
            {
                abr = iter.Current.Value;
                if (abr.isCache && abr.HaveTag(tag))
                {
                    abr.RemoveTag(tag);
                }
            }
        }

        protected void RemoveAssetsTag(int tag)
        {
            if (m_Assets.Count == 0)
            {
                return;
            }

            AssetReference ar = null;
            var iter = m_Assets.GetEnumerator();
            while (iter.MoveNext())
            {
                ar = iter.Current.Value;
                if (ar.isCache && ar.HaveTag(tag))
                {
                    ar.RemoveTag(tag);
                }
            }
        }
#endregion

        /// <summary>
        /// remove asset's asset bundle reference
        /// </summary>
        public void BreakAssetsBundleReferenceAll()
        {
            if (m_Assets.Count == 0)
            {
                return;
            }

            AssetReference ar = null;
            var iter = m_Assets.GetEnumerator();
            while (iter.MoveNext())
            {
                ar = iter.Current.Value;
                if (ar.isCache)
                {
                    ar.ReleaseBundleReference();
                }
            }
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
            if (m_Assets.Count == 0)
            {
                return;
            }

            AssetReference ar = null;
            var iter = m_Assets.GetEnumerator();
            while (iter.MoveNext())
            {
                ar = iter.Current.Value;
                if (ar.isCache && ar.HaveTag(tag))
                {
                    ar.ReleaseBundleReference();
                }
            }
        }

#region loaded manage
        void OnAssetBundleBeforeLoaded(AssetBundleLoader loader)
        {
            AssetBundleReference abr = loader.result;
            if (abr != null)
            {
                m_AssetBundles[abr.name] = abr;

                if (loader.cacheLoadedAsset)
                {
                    abr.Cache();
                }
                else
                {
                    abr.isCache = false;
                }
                abr.onDispose += OnAssetBundleDispose;
            }

            AssetBundleInfo info = loader.info;
            if (info != null)
            {
                if (m_LoadingAssetBundleLoaders.ContainsKey(info.fullName))
                {
                    m_LoadingAssetBundleLoaders.Remove(info.fullName);
                }
            }
            else
            {
                string key = null;
                foreach (var iter in m_LoadingAssetBundleLoaders)
                {
                    if (iter.Value == loader)
                    {
                        key = iter.Key;
                    }
                }

                if (!string.IsNullOrEmpty(key))
                {
                    m_LoadingAssetBundleLoaders.Remove(key);
                }
            }
        }

        void OnAssetBundleAfterLoaded(AssetBundleLoader loader)
        {
            m_LoaderManager.ReleaseLoader(loader);
        }

        void OnAssetBundleDispose(AssetBundleReference abr)
        {
            m_AssetBundles.Remove(abr.name);
        }

        void OnAssetBeforeLoaded(AssetLoader loader)
        {
            AssetReference ar = loader.result;
            if (ar != null)
            {
                m_Assets[ar.name] = ar;
                //asset loader always standalone
                ar.Cache();
                ar.onDispose += OnAssetDispose;
                //remove from loading
                if (m_LoadingAssetLoaders.ContainsKey(ar.name))
                {
                    m_LoadingAssetLoaders.Remove(ar.name);
                }
            }
            else if(loader.info!=null)
            {
                //remove from loading
                if (m_LoadingAssetLoaders.ContainsKey(loader.info.fullName))
                {
                    m_LoadingAssetLoaders.Remove(loader.info.fullName);
                }
            }
            else
            {
                string key = null;
                foreach(var iter in m_LoadingAssetLoaders)
                {
                    if (iter.Value == loader)
                    {
                        key = iter.Key;
                    }
                }

                if (!string.IsNullOrEmpty(key))
                {
                    m_LoadingAssetLoaders.Remove(key);
                }
            }
        }

        void OnAssetAfterLoaded(AssetLoader loader)
        {
            if (loader.autoReleaseBundle)
            {
                AssetReference ar = loader.result;
                if (ar != null)
                {
                    ar.ReleaseBundleReference();
                }
            }

            m_LoaderManager.ReleaseLoader(loader);
        }

        void OnAssetDispose(AssetReference ar)
        {
            m_Assets.Remove(ar.name);
        }

#endregion



        public IInfoManager infoManager
        {
            get { return m_InfoManager; }
            set {m_InfoManager = value;}
        }

        public LoaderManager loaderManager
        {
            get { return m_LoaderManager; }
            set { m_LoaderManager = value; }
        }

        public IRequestManager requestManager
        {
            get { return m_RequestManager; }
            set { m_RequestManager = value; }
        }

        public Dictionary<string,AssetBundleReference> assetBundles
        {
            get
            {
                return m_AssetBundles;
            }
        }

        public Dictionary<string,AssetReference> assets
        {
            get
            {
                return m_Assets;
            }
        }
    }
}
