using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetManager:MonoBehaviour
    {
        int m_MaxActiveLoader=5;
        int m_CurrentActiveLoader=0;
        List<Loader> m_ActivesLoaders=ListPool<Loader>.Get();
        List<int> m_TickFinished= ListPool<int>.Get();
        List<Loader> m_PrepareLoaders= ListPool<Loader>.Get();

        //all loaded assets
        Dictionary<string,AssetBundleReference> m_Assets =new Dictionary<string, AssetBundleReference>();

        public void Init()
        {
            Application.lowMemory += OnLowMemory;
        }

        public void Clean()
        {
            ListPool<Loader>.Release(m_ActivesLoaders);
            ListPool<int>.Release(m_TickFinished);
            ListPool<Loader>.Release(m_PrepareLoaders);
            m_Assets.Clear();
        }

        public Loader Load(string path)
        {
            Loader loader = CreateLoader(path);

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
                    AssetBundleReference assetBundleRef = loader.GetResult();
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

        protected Loader CreateLoader(string path)
        {
            return null;
        }

        void OnLowMemory()
        {
            UnloadUnuseds();
        }

        public void UnloadUnuseds()
        {
            if (m_Assets.Count == 0)
            {
                return;
            }
            AssetBundleReference abr=null;
            List<string> keys = ListPool<string>.Get();
            keys.AddRange(m_Assets.Keys);

            for(int i=0,l=keys.Count;i< l;++i)
            {
                abr = m_Assets[keys[i]];
                if (abr.isUnused())
                {
                    abr.Dispose();
                    m_Assets.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }

        public void UnloadUnuseds(string tag)
        {
            if (m_Assets.Count == 0)
            {
                return;
            }

            AssetBundleReference abr = null;
            List<string> keys = ListPool<string>.Get();
            keys.AddRange(m_Assets.Keys);

            for (int i = 0, l = keys.Count; i < l; ++i)
            {
                abr = m_Assets[keys[i]];
                if (abr.isUnused() && abr.HaveTag(tag))
                {
                    abr.Dispose();
                    m_Assets.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }

        public void UnloadUnuseds(int level)
        {
            if (m_Assets.Count == 0)
            {
                return;
            }

            AssetBundleReference abr = null;
            List<string> keys = ListPool<string>.Get();
            keys.AddRange(m_Assets.Keys);

            for (int i = 0, l = keys.Count; i < l; ++i)
            {
                abr = m_Assets[keys[i]];
                if (abr.isUnused() && abr.MatchLevel(level))
                {
                    abr.Dispose();
                    m_Assets.Remove(keys[i]);
                }
            }
            ListPool<string>.Release(keys);
        }
    }
}
