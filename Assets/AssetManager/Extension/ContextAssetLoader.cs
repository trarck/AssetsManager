using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage.Extension
{
    public class ContextAssetLoader
    {
        static Dictionary<Context, ContextAssetLoader> s_CacheContextAssetLoaders;

        static ObjectPool<ContextAssetLoader> s_ContextAssetLoaderPool = new ObjectPool<ContextAssetLoader>(null, (o) => { o.Clean(); });

        Context m_Context=null;

        HashSet<AssetLoader> m_AssetLoaders= new HashSet<AssetLoader>();
        HashSet<AssetBundleLoader> m_AssetBundleLoaders = new HashSet<AssetBundleLoader>();

        public Context context
        {
            set
            {
                if (m_Context != null)
                {
                    m_Context.onDisable -= OnContextDisable;
                }
                m_Context = value;
                if (m_Context!=null)
                {
                    m_Context.onDisable += OnContextDisable;
                }
            }
        }

        public void LoadAsset(string path, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            LoadAsset(path, 0, null, completeHandle, autoReleaseBundle);
        }

        public void LoadAsset<T>(string path, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            LoadAsset(path, 0, typeof(T), completeHandle, autoReleaseBundle);
        }

        public void LoadAsset<T>(string path, string tag, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            LoadAsset(path, 0, typeof(T), completeHandle, autoReleaseBundle);
        }

        public void LoadAsset(string path, int tag, Type type, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            if (m_Context != null && m_Context.enable)
            {
                //这里暂时使用匿名函数。
                AssetLoader loader = AssetManager.Instance.LoadAsset(path, tag, type, completeHandle, autoReleaseBundle);
                loader.onBeforeComplete += OnAssetBeforeComplete;
                m_AssetLoaders.Add(loader);
            }
        }

        public void LoadAssetBundle(string path, bool cacheLoadedAsset, Action<AssetBundleReference> completeHandle = null)
        {
            LoadAssetBundle(path, 0, cacheLoadedAsset, completeHandle);
        }

        public void LoadAssetBundle(string path, int tag, bool cacheLoadedAsset, Action<AssetBundleReference> completeHandle = null)
        {
            if (m_Context != null && m_Context.enable)
            {
                //这里暂时使用匿名函数。
                AssetBundleLoader loader = AssetManager.Instance.LoadAssetBundle(path, tag, cacheLoadedAsset, completeHandle);
                loader.onBeforeComplete += OnAssetBundleBeforeComplete;
                m_AssetBundleLoaders.Add(loader);
            }
        }

        void OnAssetBeforeComplete(AssetLoader loader)
        {
            loader.onBeforeComplete -= OnAssetBeforeComplete;
            m_AssetLoaders.Remove(loader);
        }

        void OnAssetBundleBeforeComplete(AssetBundleLoader loader)
        {
            loader.onBeforeComplete -= OnAssetBundleBeforeComplete;
            m_AssetBundleLoaders.Remove(loader);
        }

        void OnContextDisable()
        {
            m_Context.onDisable -= OnContextDisable;

            if (s_CacheContextAssetLoaders.ContainsKey(m_Context))
            {
                s_CacheContextAssetLoaders.Remove(m_Context);
            }

            foreach(var loader in m_AssetLoaders)
            {
                loader.Abort();
            }

            m_Context = null;

            s_ContextAssetLoaderPool.Release(this);
        }

        public void Clean()
        {
            if (m_Context != null)
            {
                m_Context.onDisable -= OnContextDisable;
                m_Context = null;
            }

            if (m_AssetLoaders != null && m_AssetLoaders.Count > 0)
            {
                foreach (var loader in m_AssetLoaders)
                {
                    loader.onBeforeComplete -= OnAssetBeforeComplete;
                }
                m_AssetLoaders.Clear();
            }

            if (m_AssetBundleLoaders != null && m_AssetBundleLoaders.Count > 0)
            {
                foreach (var loader in m_AssetBundleLoaders)
                {
                    loader.onBeforeComplete -= OnAssetBundleBeforeComplete;
                }
                m_AssetBundleLoaders.Clear();
            }
        }
        
        public static ContextAssetLoader Create(Context context)
        {
            if (context == null)
            {
                return null;
            }

            ContextAssetLoader loader = null;
            if(!s_CacheContextAssetLoaders.TryGetValue(context,out loader))
            {
                loader = s_ContextAssetLoaderPool.Get();
                loader.context = context;
                s_CacheContextAssetLoaders[context] = loader;
            }
            return loader;
        }

        public static void Release(Context context)
        {
            if (context == null)
            {
                return;
            }

            ContextAssetLoader loader = null;
            if (s_CacheContextAssetLoaders.TryGetValue(context, out loader))
            {
                s_CacheContextAssetLoaders.Remove(context);
                s_ContextAssetLoaderPool.Release(loader);
            }
        }
    }
}
