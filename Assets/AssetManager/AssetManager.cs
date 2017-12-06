﻿using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetManager:MonoBehaviour
    {
        int m_MaxActiveLoader=5;
        int m_CurrentActiveLoader=0;
        List<Loader> m_ActivesLoaders=new List<Loader>();
        List<int> m_TickFinished=new List<int>();
        List<Loader> m_PrepareLoaders=new List<Loader>();

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
            //check loader 
            CheckLoaderTick();

            //start new loader
            CheckAndStartLoaders();
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

        protected Loader CreateLoader(string path)
        {

            return null;
        }
    }
}
