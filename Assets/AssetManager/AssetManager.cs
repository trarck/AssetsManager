using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetManager:MonoBehaviour
    {
        int m_MaxActiveLoader=5;
        List<Loader> m_ActivesLoaders=ListPool<Loader>.Get();
        List<int> m_TickFinished= ListPool<int>.Get();
        List<Loader> m_PrepareLoaders= ListPool<Loader>.Get();

        //all loaded  asset bundles
        Dictionary<string,AssetBundleReference> m_AssetBundles =new Dictionary<string, AssetBundleReference>();

        //all loaded  asset bundles.usefull preload
        Dictionary<string, AssetReference> m_Assets = new Dictionary<string, AssetReference>();

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
            return LoadAssetBundle(path,null,0,completeHandle);
        }

        public AssetBundleLoader LoadAssetBundle(string path,string tag, int level, Action<AssetBundleReference> completeHandle)
        {
            AssetBundleLoader loader = null;

            if (m_AssetBundles.ContainsKey(path))
            {
                AssetBundleReference abr = m_AssetBundles[path];

                loader = m_LoaderManager.CreateAssetBundleLoader(path);
                loader.forceDone = true;
                loader.result = abr;

                //refresh 
                abr.level = level;
                abr.AddTag(tag);

                if (completeHandle != null)
                {
                    completeHandle(abr);
                }
            }
            else
            {
                loader = m_LoaderManager.CreateAssetBundleLoader(path);
                loader.paramLevel = level;
                loader.paramTag = tag;
                loader.onLoaded += DoAssetBundleLoaded;
                loader.onComplete += completeHandle;

                AddLoader(loader);
            }
            return loader;
        }

        public AssetLoader LoadAsset(string path,string tag, int level, Action<AssetReference> completeHandle)
        {
            AssetLoader loader = null;

            if (m_Assets.ContainsKey(path))
            {
                AssetReference ar = m_Assets[path];
                loader = m_LoaderManager.CreateAssetLoader(path);
                loader.forceDone = true;
                loader.result = ar;

                //refresh
                ar.level = level;
                ar.AddTag(tag);

                if (completeHandle != null)
                {
                    completeHandle(ar);
                }
            }
            else
            {
                loader = m_LoaderManager.CreateAssetLoader(path);
                loader.paramLevel = level;
                loader.paramTag = tag;
                loader.onLoaded += DoAssetLoaded;
                loader.onComplete += completeHandle;

                if (!string.IsNullOrEmpty(loader.info.bundleName))
                {
                    LoadAssetBundle(loader.info.bundleName, (abr) =>
                    {
                        loader.assetBundleReference = abr;
                        AddLoader(loader);
                    });
                }
                else
                {
                    AddLoader(loader);
                }
            }

            return loader;
        }

        void AddLoader(Loader loader)
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

        void OnLowMemory()
        {
            UnloadUnuseds();
        }

        public void UnloadUnuseds()
        {
            UnloadUnusedAssets();
            UnloadUnusedBundles();
        }

        public void UnloadUnuseds(string tag)
        {
            UnloadUnusedAssets(tag);
            UnloadUnusedBundles(tag);
        }

        public void UnloadUnuseds(int level)
        {
            UnloadUnusedAssets(level);
            UnloadUnusedBundles(level);
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

        public void UnloadUnusedBundles(string tag)
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
                if (abr.isUnused() && abr.HaveTag(tag))
                {
                    abr.Dispose();
                    m_AssetBundles.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }

        public void UnloadUnusedBundles(int level)
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
                if (abr.isUnused() && abr.MatchLevel(level))
                {
                    abr.Dispose();
                    m_AssetBundles.Remove(keys[i]);
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
                if (ar.isUnused() && ar.HaveTag(tag))
                {
                    ar.Dispose();
                    m_Assets.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }

        public void UnloadUnusedAssets(int level)
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
                if (ar.isUnused() && ar.MatchLevel(level))
                {
                    ar.Dispose();
                    m_Assets.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }

        void DoAssetBundleLoaded(AssetBundleLoader loader)
        {
            AssetBundleReference abr = loader.result;
            if (abr != null)
            {
                m_AssetBundles[abr.assetBundleName] = abr;
            }
        }

        void DoAssetLoaded(AssetLoader loader)
        {
            AssetReference ar = loader.result;
            if (ar != null)
            {
                m_Assets[ar.assetPath] = ar;
            }
        }

        public InfoManager infoManager
        {
            get { return m_InfoManager; }
        }
    }
}
