using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
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

        InfoManager m_InfoManager;
        LoaderManager m_LoaderManager;
        RequestManager m_RequestManager;

        bool m_Inited = false;

        void Awake()
        {
            Init();
        }

        public void Init(Action<bool> callback=null)
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

            Application.lowMemory += OnLowMemory;

            //add search paths
            AssetPaths.AddSearchPath(AssetPaths.Combine(Application.persistentDataPath, AssetPaths.bundlesPath));
            AssetPaths.AddSearchPath(Application.persistentDataPath);
#if UNITY_EDITOR
            //bunlde out path
            AssetPaths.AddSearchPath(
                AssetPaths.Combine(
                    System.IO.Path.GetFullPath("."),
                    AssetPaths.bundleOutPaths,
                    UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString()
                )
            );
#endif

            m_LoaderManager = new LoaderManager(this);
            m_RequestManager = new RequestManager(this);

            m_InfoManager = new InfoManager(this);
            if (callback != null)
            {
                m_InfoManager.onInitComplete += callback;
            }
            
            m_InfoManager.Load(AssetPaths.GetFullPath(AssetPaths.bundleManifestFile));
        }

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

        public AssetBundleLoader LoadAssetBundle(string path,int tag,bool cacheLoadedAsset, Action<AssetBundleReference> completeHandle=null)
        {
            AssetBundleLoader loader = null;

            if (string.IsNullOrEmpty(path))
            {
                return loader;
            }

            if (m_AssetBundles.ContainsKey(path))
            {
                #if ASSETMANAGER_LOG
                Debug.Log("LoadAssetBundle asset bundle is loaded " + path + "," + Time.frameCount);
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
                    completeHandle(abr);
                }
            }
            else
            {
                if (m_LoadingAssetBundleLoaders.ContainsKey(path))
                {
                    #if ASSETMANAGER_LOG
                    Debug.Log("LoadAssetBundle using loading loader " + path + "," + Time.frameCount);
                    #endif
                    loader = m_LoadingAssetBundleLoaders[path];
                }
                else
                {
                    #if ASSETMANAGER_LOG
                    Debug.Log("LoadAssetBundle create new loader " + path + "," + Time.frameCount);
                    #endif
                    loader = m_LoaderManager.CreateAssetBundleAsyncLoader(path);
                    m_LoadingAssetBundleLoaders[path] = loader;
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
                    loader.onAfterComplete += OnAssetBundleLoaded;
                    loader.state = Loader.State.Inited;
                    m_LoaderManager.ActiveLoader(loader);                    
                }                
            }

            return loader;
        }

        public AssetBundleReference LoadAssetBundleSync(string path, bool cacheLoadedAsset = true)
        {
            return LoadAssetBundleSync(path, 0, cacheLoadedAsset);
        }

        public AssetBundleReference LoadAssetBundleSync(string path, int tag, bool cacheLoadedAsset = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            AssetBundleReference abr = null;

            if (m_AssetBundles.ContainsKey(path))
            {
                #if ASSETMANAGER_LOG
                Debug.LogFormat("LoadAssetBundleSync bundle is loaded {0},{1}", path, Time.frameCount);
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
                    Debug.LogErrorFormat("LoadAssetBundleSync async loader is active {0},{1}", path, Time.frameCount);
                    //TODO Stop async
                    return null;
                }
                else
                {
                    #if ASSETMANAGER_LOG
                    Debug.LogFormat("LoadAssetBundleSync create new loader {0},{1}", path, Time.frameCount);
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
                        OnAssetBundleLoaded(loader);
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
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="type"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetLoader LoadAsset(string path,int tag, Type type,Action<AssetReference> completeHandle=null,bool autoReleaseBundle=true)
        {
            if (!string.IsNullOrEmpty(path))
            {
                path = AssetPaths.AddAssetPrev(path);
            }

            AssetLoader loader = null;

            if (m_Assets.ContainsKey(path))
            {
                #if ASSETMANAGER_LOG
                Debug.Log("LoadAsset asset is loaded "+path+","+Time.frameCount);
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
                    completeHandle(ar);
                }
            }
            else
            {
                if (m_LoadingAssetLoaders.ContainsKey(path))
                {
#if ASSETMANAGER_LOG
                    Debug.Log("LoadAsset using loading loader " + path + "," + Time.frameCount);
#endif
                    loader = m_LoadingAssetLoaders[path];
                }
                else
                {
#if ASSETMANAGER_LOG
                    Debug.Log("LoadAsset create new loader " + path + "," + Time.frameCount);
#endif
                    loader = m_LoaderManager.CreateAssetAsyncLoader(path);
                    m_LoadingAssetLoaders[path] = loader;
                }
                
                loader.AddParamTag(tag);
                loader.onComplete += completeHandle;

                if (type != null)
                {
                    loader.type = type;
                }

                if (!autoReleaseBundle)
                {
                    loader.autoReleaseBundle = autoReleaseBundle;
                }

                if (loader.state == Loader.State.Idle)
                {
                    loader.onBeforeComplete += OnAssetBeforeLoaded;
                    loader.onAfterComplete += OnAssetAfterLoaded;
                    loader.state = Loader.State.Inited;

                    m_LoaderManager.ActiveLoader(loader);
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
                Debug.LogErrorFormat("LoadAsset no alias {0} find ", alias);
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
#if ASSETMANAGER_LOG
                Debug.Log("LoadAssetSync asset is loaded " + path + "," + Time.frameCount);
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
#if ASSETMANAGER_LOG
                    Debug.Log("LoadAssetSync async load staring " + path + "," + Time.frameCount);
#endif
                    //TODO Stop async loader
                    return null;
                }
                else
                {
#if ASSETMANAGER_LOG
                    Debug.Log("LoadAssetSync create new loader " + path + "," + Time.frameCount);
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

        public void LoadAssets(List<string> assets, Action<Dictionary<string, AssetReference>> callback)
        {
            Dictionary<string, AssetReference> assetReferences = new Dictionary<string, AssetReference>();
            int needCount = assets.Count;
            bool needCallback = true;

            for (int i=0,l=assets.Count;i<l;++i)
            {
                var asset = assets[i];
                if (!string.IsNullOrEmpty(asset))
                {
                    needCallback = false;
                    AssetManager.Instance.LoadAsset(asset, (assetReference) => {
                        assetReferences[asset] = assetReference;
                        //all finished
                        if (--needCount <= 0)
                        {
                            callback(assetReferences);
                        }
                    });
                }
                else
                {
                    --needCount;
                }
            }

            if (needCount == 0 && needCallback)
            {
                callback(null);
            }
        }
        #endregion

        #region Yield Load Asset Bundle
        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <param name="path"></param>
        /// <param name="standalone"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public BundleLoaderEnumerator YieldLoadAssetBundle(string path, bool standalone, Action<AssetBundleReference> completeHandle = null)
        {
            return YieldLoadAssetBundle(path, 0, standalone,  completeHandle);
        }

        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="standalone"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public BundleLoaderEnumerator YieldLoadAssetBundle(string path, int tag, bool standalone, Action<AssetBundleReference> completeHandle = null)
        {
            AssetBundleLoader loader = LoadAssetBundle(path, tag, standalone, completeHandle);
            if (loader != null)
            {
                loader.autoRelease = false;
                return new BundleLoaderEnumerator(loader);
            }
            return null;
        }

#endregion

#region Yield Load Asset
        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <param name="path"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetLoaderEnumerator YieldLoadAsset(string path, Action<AssetReference> completeHandle = null)
        {
            return YieldLoadAsset(path, 0, null, completeHandle);
        }

        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetLoaderEnumerator YieldLoadAsset<T>(string path, Action<AssetReference> completeHandle = null)
        {
            return YieldLoadAsset(path, 0, typeof(T), completeHandle);
        }

        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetLoaderEnumerator YieldLoadAsset<T>(string path, string tag, Action<AssetReference> completeHandle = null)
        {
            return YieldLoadAsset(path, 0, typeof(T), completeHandle);
        }

        /// <summary>
        /// 使用yield要注意loader的释放。使用using或手动调用dispose
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tag"></param>
        /// <param name="type"></param>
        /// <param name="completeHandle"></param>
        /// <returns></returns>
        public AssetLoaderEnumerator YieldLoadAsset(string path, int tag, Type type, Action<AssetReference> completeHandle = null)
        {
            AssetLoader loader = LoadAsset(path, tag, type, completeHandle);
            if (loader != null)
            {
                loader.autoRelease = false;
                return new AssetLoaderEnumerator(loader);
            }
            return null;
        }

#endregion

        void Update()
        {
            if (m_RequestManager!=null)
            {
                m_RequestManager.Update();
            }

#if ASSET_BUNDLE_REMOTE
            if (m_InfoManager != null)
            {
                m_InfoManager.Update(Time.deltaTime);
            }
#endif
        }

        #region exter function
        public AssetBundleLoader LoadScene(string path, int tag, Action<AssetBundleReference> completeHandle)
        {
            AssetInfo info = m_InfoManager.FindAssetInfo(path);
            if (info != null)
            {
                return LoadAssetBundle(info.bundleName, tag, true, completeHandle);
            }
            else
            {
                return LoadAssetBundle(path, tag, true, completeHandle);
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
        void OnAssetBundleLoaded(AssetBundleLoader loader)
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
                    if (loader.autoRelease)
                    {
                        LoaderPool.Release(loader);
                    }
                    m_LoadingAssetBundleLoaders.Remove(info.fullName);
                }
            }

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

            if (loader.autoRelease)
            {
                LoaderPool.Release(loader);
            }
        }

        void OnAssetDispose(AssetReference ar)
        {
            m_Assets.Remove(ar.name);
        }

        #endregion

#if ASSET_BUNDLE_REMOTE
        private void OnApplicationPause(bool pause)
        {
            if (m_InfoManager != null)
            {
                m_InfoManager.SaveLocalAssetBundleInfo();
            }
        }

        private void OnApplicationQuit()
        {
            if (m_InfoManager != null)
            {
                m_InfoManager.SaveLocalAssetBundleInfo();
            }   
        }
#endif
        public InfoManager infoManager
        {
            get { return m_InfoManager; }
        }

        public LoaderManager loaderManager
        {
            get { return m_LoaderManager; }
        }

        public RequestManager requestManager
        {
            get { return m_RequestManager; }
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
