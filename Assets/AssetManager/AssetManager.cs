using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetManager : UnitySingleton<AssetManager>
    {
        int m_MaxActiveLoader = 5;
        List<Loader> m_ActivesLoaders = ListPool<Loader>.Get();
        List<int> m_TickFinished = ListPool<int>.Get();
        List<Loader> m_PrepareLoaders = ListPool<Loader>.Get();

        //all loaded  asset bundles
        Dictionary<string, AssetBundleReference> m_AssetBundles = new Dictionary<string, AssetBundleReference>();

        //all loaded  asset bundles.usefull preload
        Dictionary<string, AssetReference> m_Assets = new Dictionary<string, AssetReference>();

        //loading loaders
        Dictionary<string, AssetBundleLoader> m_LoadingAssetBundleLoaders = new Dictionary<string, AssetBundleLoader>();
        Dictionary<string, AssetLoader> m_LoadingAssetLoaders = new Dictionary<string, AssetLoader>();

        InfoManager m_InfoManager;
        LoaderManager m_LoaderManager;

        public void Init(Action<bool> callback=null)
        {
            Application.lowMemory += OnLowMemory;

            //add search paths
            AssetPaths.AddSearchPath(AssetPaths.Combine(Application.persistentDataPath, AssetPaths.bundlesPath));
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

            m_InfoManager = new InfoManager(this);
            if (callback != null)
            {
                m_InfoManager.onInitComplete += callback;
            }
            
            m_InfoManager.Load(AssetPaths.GetFullPath(AssetPaths.bundleManifestFile));
        }

        public void Clean()
        {
            ListPool<Loader>.Release(m_ActivesLoaders);
            ListPool<int>.Release(m_TickFinished);
            ListPool<Loader>.Release(m_PrepareLoaders);
            m_AssetBundles.Clear();
            m_Assets.Clear();
        }

        #region load asset bundle

        public AssetBundleLoader LoadAssetBundle(string path, bool standalone, Action<AssetBundleReference> completeHandle=null)
        {
            return LoadAssetBundle(path,null, standalone, completeHandle);
        }

        public AssetBundleLoader LoadAssetBundle(string path,string tag,bool standalone, Action<AssetBundleReference> completeHandle=null)
        {
            AssetBundleLoader loader = null;

            if (string.IsNullOrEmpty(path))
            {
                return loader;
            }

            if (m_AssetBundles.ContainsKey(path))
            {
                Debug.Log("LoadAssetBundle asset bundle is loaded " + path + "," + Time.frameCount);
                //asset bundle is loaded
                AssetBundleReference abr = m_AssetBundles[path];

                //refresh 
                abr.AddTag(tag);

                if (standalone)
                {
                    abr.Chain();
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
                    Debug.Log("LoadAssetBundle using loading loader " + path + "," + Time.frameCount);
                    loader = m_LoadingAssetBundleLoaders[path];
                }
                else
                {
                    Debug.Log("LoadAssetBundle create new loader " + path + "," + Time.frameCount);
                    loader = m_LoaderManager.CreateAssetBundleAsyncLoader(path);
                    m_LoadingAssetBundleLoaders[path] = loader;
                }
                
                loader.AddParamTag(tag);

                if(loader.standalone==false && standalone)
                {
                    loader.standalone = true;
                }

                if (completeHandle != null)
                {
                    loader.onComplete += completeHandle;
                }

                if (loader.state == Loader.State.Idle)
                {
                    loader.onBeforeComplete += OnAssetBundleLoaded;
                    loader.state = Loader.State.Inited;
                    ActiveLoader(loader);
                }                
            }

            return loader;
        }

        public AssetBundleReference LoadAssetBundleSync(string path, bool standalone=true)
        {
            return LoadAssetBundleSync(path, null, standalone);
        }

        public AssetBundleReference LoadAssetBundleSync(string path, string tag, bool standalone=true)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            AssetBundleReference abr = null;

            if (m_AssetBundles.ContainsKey(path))
            {
                Debug.LogFormat("LoadAssetBundleSync bundle is loaded {0},{1}", path, Time.frameCount);
                abr = m_AssetBundles[path];
                //refresh 
                abr.AddTag(tag);

                if (standalone)
                {
                    abr.Chain();
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
                    Debug.LogFormat("LoadAssetBundleSync create new loader {0},{1}", path, Time.frameCount);
                    AssetBundleSyncLoader loader = m_LoaderManager.CreateAssetBundleSyncLoader(path);
                    if (loader != null)
                    {
                        loader.state = Loader.State.Inited;
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
        public AssetLoader LoadAsset(string path, Action<AssetReference> completeHandle=null)
        {
            return LoadAsset(path, null,null, completeHandle);
        }

        public AssetLoader LoadAsset<T>(string path, Action<AssetReference> completeHandle=null)
        {
            return LoadAsset(path, null,  typeof(T), completeHandle);
        }

        public AssetLoader LoadAsset<T>(string path, string tag,Action<AssetReference> completeHandle=null)
        {
            return LoadAsset(path, null,  typeof(T), completeHandle);
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
        public AssetLoader LoadAsset(string path,string tag, Type type,Action<AssetReference> completeHandle=null)
        {
            if (!string.IsNullOrEmpty(path))
            {
                path = AssetPaths.AddAssetPrev(path);
            }

            AssetLoader loader = null;

            if (m_Assets.ContainsKey(path))
            {
                Debug.Log("LoadAsset asset is loaded "+path+","+Time.frameCount);
                AssetReference ar = m_Assets[path];

                //refresh
                ar.AddTag(tag);

                //in chain will check is chained.
                ar.Chain();

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
                    Debug.Log("LoadAsset using loading loader " + path + "," + Time.frameCount);
                    loader = m_LoadingAssetLoaders[path];
                }
                else
                {
                    Debug.Log("LoadAsset create new loader " + path + "," + Time.frameCount);
                    loader = m_LoaderManager.CreateAssetAsyncLoader(path);
                    m_LoadingAssetLoaders[path] = loader;
                }
                
                loader.AddParamTag(tag);
                loader.onComplete += completeHandle;

                if (type != null)
                {
                    loader.type = type;
                }

                if (loader.state == Loader.State.Idle)
                {
                    loader.onBeforeComplete += OnAssetLoaded;
                    loader.state = Loader.State.Inited;

                    ActiveLoader(loader);
                }
            }

            return loader;
        }

        public AssetLoader LoadAssetWithAlias(string alias, string tag, Type type, Action<AssetReference> completeHandle=null)
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
            return LoadAssetSync(path, null, null);
        }

        public AssetReference LoadAssetSync(string path, string tag)
        {
            return LoadAssetSync(path, tag, null);
        }

        public AssetReference LoadAssetSync(string path, string tag, Type type)
        {
            if (!string.IsNullOrEmpty(path))
            {
                path = AssetPaths.AddAssetPrev(path);
            }

            AssetReference ar = null;

            AssetLoader loader = null;

            if (m_Assets.ContainsKey(path))
            {
                Debug.Log("LoadAssetSync asset is loaded " + path + "," + Time.frameCount);
                ar = m_Assets[path];

                //refresh
                ar.AddTag(tag);

                //in chain will check is chained.
                ar.Chain();
            }
            else
            {
                if (m_LoadingAssetLoaders.ContainsKey(path))
                {
                    Debug.Log("LoadAssetSync async load staring " + path + "," + Time.frameCount);
                    //TODO Stop async loader
                    return null;
                }
                else
                {
                    Debug.Log("LoadAssetSync create new loader " + path + "," + Time.frameCount);
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
                OnAssetLoaded(loader);
            }

            return ar;
        }

        #endregion

        #region loader manage
        void ActiveLoader(Loader loader)
        {
            if (m_ActivesLoaders.Count < m_MaxActiveLoader)
            {
                m_ActivesLoaders.Add(loader);
                loader.Start();
            }
            else
            {
                m_PrepareLoaders.Add(loader);
            }
        }

        void Update()
        {
            //start new loader
            CheckAndStartLoaders();

            //check loader 
            CheckLoaderTick();
        }

        protected void CheckLoaderTick()
        {
            m_TickFinished.Clear();

            try
            {
                Loader loader = null;
                for (int i = 0, l = m_ActivesLoaders.Count; i < l; ++i)
                {
                    loader = m_ActivesLoaders[i];
                    if (loader.isDone)
                    {
                        m_TickFinished.Add(i);
                        loader.Complete();
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

            //remove finished loader
            if (m_TickFinished.Count > 0)
            {
                for (int i = m_TickFinished.Count - 1; i >= 0; --i)
                {
                    m_ActivesLoaders.RemoveAt(m_TickFinished[i]);
                }
            }
        }

        protected void CheckAndStartLoaders()
        {
            if (m_PrepareLoaders.Count > 0 && m_ActivesLoaders.Count < m_MaxActiveLoader)
            {
                int activeCount = m_MaxActiveLoader - m_ActivesLoaders.Count;
                activeCount = activeCount > m_PrepareLoaders.Count ? m_PrepareLoaders.Count : activeCount;
                Loader loader = null;
                for (int i = 0; i < activeCount; ++i)
                {
                    loader = m_PrepareLoaders[i];
                    m_ActivesLoaders.Add(loader);
                    loader.Start();
                }
            }
        }
        #endregion


        #region exter function
        public AssetBundleLoader LoadScene(string path, string tag, Action<AssetBundleReference> completeHandle)
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

        public void UnloadUnuseds(string tag,bool removeTag=true)
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
                if (abr.inChain)
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
                            if (sub.inChain && !checkings.Contains(sub.name))
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

        public void UnloadUnusedBundles(string tag)
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
                            if (sub.inChain && (sub.tagCount == 0 || sub.HaveTag(tag)) && !checkings.Contains(sub.name))
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
                if (abr.HaveTag(tag) && abr.inChain)
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

        public void UnloadUnusedAssets(string tag)
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
                abr.UnChain();
            }
        }

        void UnusedAllAssets()
        {
            foreach(var iter in m_Assets)
            {
                AssetReference ar = iter.Value;
                ar.UnChain();
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
                abr.UnChain();
            }
        }

        public void UnusedAssetBundle(AssetBundleReference abr)
        {
            if (abr != null)
            {
                abr.UnChain();
            }
        }

        public void UnusedAsset(string assetName)
        {
            if (m_Assets.ContainsKey(assetName))
            {
                AssetReference ar = m_Assets[assetName];
                ar.UnChain();
            }
        }

        public void UnusedAsset(AssetReference ar)
        {
            if (ar != null)
            {
                ar.UnChain();
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
        public void RemoveTags(string tag)
        {
            RemoveAssetsTag(tag);
            RemoveAssetBundlesTag(tag);
        }

        protected void RemoveAssetBundlesTag(string tag)
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
                if (abr.inChain && abr.HaveTag(tag))
                {
                    abr.RemoveTag(tag);
                }
            }
        }

        protected void RemoveAssetsTag(string tag)
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
                if (ar.inChain && ar.HaveTag(tag))
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
                if (ar.inChain)
                {
                    ar.assetBundleReference = null;
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
        public void BreakAssetBundleReferenceByTag(string tag)
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
                if (ar.inChain && ar.HaveTag(tag))
                {
                    ar.assetBundleReference = null;
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

                if (loader.standalone)
                {
                    abr.Chain();
                }
                else
                {
                    abr.inChain = false;
                }

                if (m_LoadingAssetBundleLoaders.ContainsKey(abr.name))
                {
                    m_LoadingAssetBundleLoaders.Remove(abr.name);
                }

                abr.onDispose += OnAssetBundleDispose;
            }
        }

        void OnAssetBundleDispose(AssetBundleReference abr)
        {
            m_AssetBundles.Remove(abr.name);
        }

        void OnAssetLoaded(AssetLoader loader)
        {
            AssetReference ar = loader.result;
            if (ar != null)
            {
                m_Assets[ar.name] = ar;

                //asset loader always standalone
                ar.Chain();

                if (m_LoadingAssetLoaders.ContainsKey(ar.name))
                {
                    m_LoadingAssetLoaders.Remove(ar.name);
                }

                ar.onDispose += OnAssetDispose;
            }
        }

        void OnAssetDispose(AssetReference ar)
        {
            m_Assets.Remove(ar.name);
        }

        #endregion

        public InfoManager infoManager
        {
            get { return m_InfoManager; }
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
