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

        //all loaded assets
        Dictionary<string,AssetBundleReference> m_AssetBundles =new Dictionary<string, AssetBundleReference>();

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
        }

        public AssetBundleLoader LoadAssetBundle(string path, Action<AssetBundleReference> completeHandle)
        {
            AssetBundleLoader loader = m_LoaderManager.CreateAssetBundleLoader(path);
            loader.onLoaded += DoAssetBundleLoaded;

            loader.onComplete += completeHandle;            

            AddLoader(loader);

            return loader;
        }

        public AssetLoader LoadAsset(string path,Action<Object> completeHandle)
        {
                        
            AssetLoader loader = m_LoaderManager.CreateAssetLoader(path);
            loader.onComplete += completeHandle;

            if (!string.IsNullOrEmpty(loader.info.bundleName))
            {

            }

            AddLoader(loader);

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

        public void UnloadUnuseds(string tag)
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

        public void UnloadUnuseds(int level)
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

        public void DoAssetBundleLoaded(AssetBundleLoader loader)
        {
            AssetBundleReference abr = loader.GetResult();
            if (abr != null)
            {
                m_AssetBundles[abr.assetBundleName] = abr;
            }
        }

        public InfoManager infoManager
        {
            get { return m_InfoManager; }
        }
    }
}
