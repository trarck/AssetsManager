using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage.Extension
{
    public class ContextAssetLoader
    {
        static ObjectPool<ContextAssetLoader> s_ContextAssetLoaderPool = new ObjectPool<ContextAssetLoader>(null, (o) => { o.Clean(); });

        static Dictionary<Context, ContextAssetLoader> s_CacheContextAssetLoaders=new Dictionary<Context, ContextAssetLoader>();

        Context m_Context=null;

        HashSet<AssetLoader> m_AssetLoaders= new HashSet<AssetLoader>();
        Dictionary<AssetLoader, Action<AssetReference>> m_AssetCompleteHandles = new Dictionary<AssetLoader, Action<AssetReference>>();

        HashSet<AssetBundleLoader> m_AssetBundleLoaders = new HashSet<AssetBundleLoader>();
        Dictionary<AssetBundleLoader, Action<AssetBundleReference>> m_AssetBundleCompleteHandles = new Dictionary<AssetBundleLoader, Action<AssetBundleReference>>();

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
                m_AssetCompleteHandles[loader] = completeHandle;
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
                if (loader!=null)
                {
                    loader.onBeforeComplete += OnAssetBundleBeforeComplete;
                    m_AssetBundleLoaders.Add(loader);
                    m_AssetBundleCompleteHandles[loader] = completeHandle;
                }
            }
        }

        void OnAssetBeforeComplete(AssetLoader loader)
        {
            //check enable
            if (m_Context != null && !m_Context.enable)
            {
                OnContextDisable();
            }
            else
            {
                loader.onBeforeComplete -= OnAssetBeforeComplete;
                m_AssetLoaders.Remove(loader);
                m_AssetCompleteHandles.Remove(loader);
            }
        }

        void OnAssetBundleBeforeComplete(AssetBundleLoader loader)
        {
            //check enable
            if (m_Context != null && !m_Context.enable)
            {
                OnContextDisable();
            }
            else
            {
                loader.onBeforeComplete -= OnAssetBundleBeforeComplete;
                m_AssetBundleLoaders.Remove(loader);
                m_AssetBundleCompleteHandles.Remove(loader);
            }
        }

        void OnContextDisable()
        {
            if (s_CacheContextAssetLoaders.ContainsKey(m_Context))
            {
                s_CacheContextAssetLoaders.Remove(m_Context);
            }

            Clean();

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
                    loader.onComplete -= m_AssetCompleteHandles[loader];
                }
                m_AssetLoaders.Clear();
                m_AssetCompleteHandles.Clear();
            }

            if (m_AssetBundleLoaders != null && m_AssetBundleLoaders.Count > 0)
            {
                foreach (var loader in m_AssetBundleLoaders)
                {
                    loader.onBeforeComplete -= OnAssetBundleBeforeComplete;
                    loader.onComplete -= m_AssetBundleCompleteHandles[loader];
                }
                m_AssetBundleLoaders.Clear();
                m_AssetBundleCompleteHandles.Clear();
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
