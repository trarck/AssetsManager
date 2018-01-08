using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetManager : MonoBehaviour
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

        //auto release pool
        Dictionary<string, ReferenceAutoRelease> m_AutoReleaseAssetBundles = new Dictionary<string, ReferenceAutoRelease>();
        Dictionary<string, ReferenceAutoRelease> m_AutoReleaseAssets = new Dictionary<string, ReferenceAutoRelease>();

        InfoManager m_InfoManager;
        LoaderManager m_LoaderManager;

        public void Init(Action callback)
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

        public AssetBundleLoader LoadAssetBundle(string path, Action<AssetBundleReference> completeHandle)
        {
            return LoadAssetBundle(path,null,completeHandle);
        }

        public AssetBundleLoader LoadAssetBundle(string path,string tag,bool autoRelease, Action<AssetBundleReference> completeHandle)
        {
            AssetBundleLoader loader = null;

            if (m_AssetBundles.ContainsKey(path))
            {
                Debug.Log("LoadAssetBundle asset bundle is loaded " + path + "," + Time.frameCount);
                //asset bundle is loaded
                AssetBundleReference abr = m_AssetBundles[path];

                loader = m_LoaderManager.CreateAssetBundleLoader(path);
                loader.forceDone = true;
                loader.result = abr;

                //refresh 
                abr.AddTag(tag);

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
                    loader = m_LoaderManager.CreateAssetBundleLoader(path);
                    m_LoadingAssetBundleLoaders[path] = loader;
                }
                
                loader.AddParamTag(tag);

                if (loader.autoRelease == false && autoRelease==true)
                {
                    loader.autoRelease = autoRelease;
                }

                loader.onComplete += completeHandle;

                if (loader.state == Loader.State.Idle)
                {
                    loader.onLoaded += OnAssetBundleLoaded;
                    loader.state = Loader.State.Inited;
                    ActiveLoader(loader);
                }                
            }
            return loader;
        }

        public AssetLoader LoadAsset(string path, Action<AssetReference> completeHandle)
        {
            return LoadAsset(path, null,null, false,completeHandle);
        }

        public AssetLoader LoadAsset<T>(string path, Action<AssetReference> completeHandle)
        {
            return LoadAsset(path, null,  typeof(T), false,completeHandle);
        }

        public AssetLoader LoadAsset<T>(string path, string tag,Action<AssetReference> completeHandle)
        {
            return LoadAsset(path, null,  typeof(T),false, completeHandle);
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
        public AssetLoader LoadAsset(string path,string tag, Type type,bool autoRelease, Action<AssetReference> completeHandle)
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
                loader = m_LoaderManager.CreateAssetLoader(path);
                loader.forceDone = true;
                loader.result = ar;

                //refresh
                ar.AddTag(tag);

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
                    loader = m_LoaderManager.CreateAssetLoader(path);
                    m_LoadingAssetLoaders[path] = loader;
                }
                
                loader.AddParamTag(tag);
                if (type != null)
                {
                    loader.type = type;
                }

                if (loader.autoRelease == false && autoRelease == true)
                {
                    loader.autoRelease = autoRelease;
                }

                loader.onComplete += completeHandle;

                if (loader.state == Loader.State.Idle)
                {
                    loader.onLoaded += OnAssetLoaded;
                    loader.state = Loader.State.Inited;

                    if (!string.IsNullOrEmpty(loader.info.bundleName))
                    {
                        LoadAssetBundle(loader.info.bundleName,tag, autoRelease,(abr) =>
                        {
                            loader.assetBundleReference = abr;
                            ActiveLoader(loader);
                        });
                    }
                    else
                    {
                        ActiveLoader(loader);
                    }
                }
            }

            return loader;
        }

        public AssetLoader LoadAssetWithAlias(string alias, string tag, Type type,bool cached, Action<AssetReference> completeHandle)
        {
            AssetInfo assetInfo = m_InfoManager.FindAssetInfoWithAlias(alias);
            if (assetInfo != null)
            {
                return LoadAsset(assetInfo.fullName, tag, type, cached, completeHandle);
            }
            else
            {
                Debug.LogErrorFormat("LoadAsset no alias {0} find ", alias);
            }
            return null;
        }

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

            //check auto release
            CheckAutoReleases();
        }

        protected void CheckLoaderTick()
        {
            m_TickFinished.Clear();

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

        protected void CheckAutoReleases()
        {
            CheckAutoReleaseAssets();
            CheckAutoReleaseBundles();
        }

        public void CheckAutoReleaseAssets()
        {
            if (m_AutoReleaseAssets.Count == 0)
            {
                return;
            }

            List<string> keys = ListPool<string>.Get();
            keys.AddRange(m_AutoReleaseAssets.Keys);

            AutoRelease autoRelease = null;
            int currentFrame = Time.frameCount;
            for (int i = 0, l = m_AutoReleaseAssets.Count; i < l; ++i)
            {
                autoRelease = m_AutoReleaseAssets[keys[i]];
                if (autoRelease.IsReleasFrame(currentFrame))
                {
                    RemoveAsset(autoRelease.key);
                    m_AutoReleaseAssets.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }

        protected void CheckAutoReleaseBundles()
        {
            if (m_AutoReleaseAssetBundles.Count == 0)
            {
                return;
            }
            List<string> keys = ListPool<string>.Get();
            keys.AddRange(m_AutoReleaseAssetBundles.Keys);

            AutoRelease autoRelease=null;
            int currentFrame = Time.frameCount;
            for (int i = 0, l = keys.Count; i < l; ++i)
            {
                autoRelease = m_AutoReleaseAssetBundles[keys[i]];
                if (autoRelease.IsReleasFrame(currentFrame))
                {
                    RemoveAssetBundle(autoRelease.key);
                    m_AutoReleaseAssetBundles.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }



        void OnLowMemory()
        {
            UnloadUnuseds();
        }

        public void UnloadUnuseds()
        {
            UnloadUnusedAssets();
            UnloadUnusedBundles();
            Resources.UnloadUnusedAssets();
        }

        public void UnloadUnuseds(string tag,bool needRemove = false)
        {
            UnloadUnusedAssets(tag, needRemove);
            UnloadUnusedBundles(tag, needRemove);
            Resources.UnloadUnusedAssets();
        }

        public void UnloadUnusedBundles()
        {
            if (m_AssetBundles.Count == 0)
            {
                return;
            }
            AssetBundleReference abr=null;
            List<string> keys = ListPool<string>.Get();
            keys.AddRange(m_AssetBundles.Keys);

            for(int i=0,l=keys.Count;i< l;++i)
            {
                abr = m_AssetBundles[keys[i]];
                if (abr.isUnused())
                {
                    abr.Dispose();
                    m_AssetBundles.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }

        public void UnloadUnusedBundles(string tag, bool needRemove = false)
        {
            if (m_AssetBundles.Count == 0)
            {
                return;
            }

            AssetBundleReference abr = null;
            List<string> keys = ListPool<string>.Get();
            keys.AddRange(m_AssetBundles.Keys);

            for (int i = 0, l = keys.Count; i < l; ++i)
            {
                abr = m_AssetBundles[keys[i]];
                if(abr.HaveTag(tag))
                {
                    if (abr.isUnused())
                    {
                        abr.Dispose();
                        m_AssetBundles.Remove(keys[i]);
                    }
                    else if (needRemove)
                    {
                        abr.RemoveTag(tag);
                    }
                }
            }
            ListPool<string>.Release(keys);
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

        public void UnloadUnusedAssets(string tag,bool needRemove=false)
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
                    else if (needRemove)
                    {
                        ar.RemoveTag(tag);
                    }
                }
            }
            ListPool<string>.Release(keys);
        }

        public void RemoveAssetBundle(string assetBundleName)
        {
            if (m_AssetBundles.ContainsKey(assetBundleName))
            {
                AssetBundleReference abr = m_AssetBundles[assetBundleName];
                m_AssetBundles.Remove(assetBundleName);
                abr.onDispose -= OnAssetBundleDispose;
                abr.Release();
            }
        }

        public void RemoveAssetBundle(AssetBundleReference abr)
        {
            if (m_AssetBundles.ContainsValue(abr))
            {
                m_AssetBundles.Remove(abr.name);
                abr.onDispose -= OnAssetBundleDispose;
                abr.Release();
            }
        }

        public void RemoveAsset(string assetName)
        {
            if (m_Assets.ContainsKey(assetName))
            {
                AssetReference ar = m_Assets[assetName];
                m_Assets.Remove(assetName);
                ar.onDispose -= OnAssetDispose;
                ar.Release();
            }
        }

        public void RemoveAsset(AssetReference ar)
        {
            if (m_Assets.ContainsValue(ar))
            {
                m_Assets.Remove(ar.name);
                ar.onDispose -= OnAssetDispose;
                ar.Release();
            }
        }

        void OnAssetBundleLoaded(AssetBundleLoader loader)
        {
            AssetBundleReference abr = loader.result;
            if (abr != null)
            {
                m_AssetBundles[abr.name] = abr;

                if (loader.autoRelease)
                {
                    AutoRelease autoRelease = new AutoRelease();
                    autoRelease.key = abr.name;
                    autoRelease.ReleaseNextFrame();
                    m_AutoReleaseAssetBundles[abr.name] = autoRelease;
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

                if (loader.autoRelease)
                {
                    AutoRelease autoRelease= new AutoRelease();
                    autoRelease.key = ar.name;
                    autoRelease.ReleaseNextFrame();
                    m_AutoReleaseAssets[ar.name] = autoRelease;
                }

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

        public InfoManager infoManager
        {
            get { return m_InfoManager; }
        }
    }
}
