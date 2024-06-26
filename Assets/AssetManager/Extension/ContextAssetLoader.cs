﻿using System;
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

        #region Asset
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
                AMDebug.LogFormat("[AssetManage]ContextAssetLoader Start load asset {0}.", path);
                //这里暂时使用匿名函数。
                AssetLoader loader = AssetManager.Instance.LoadAsset(path, tag, type, autoReleaseBundle, completeHandle).loader;
                loader.onBeforeComplete += OnAssetBeforeComplete;
                m_AssetLoaders.Add(loader);
                m_AssetCompleteHandles[loader] = completeHandle;
            }
            else
            {
                AMDebug.LogFormat("[ContextAssetLoader] Can't lad asset {0}.The context is disable", path);
            }
        }
        #endregion

        #region AssetBundle

        public void LoadAssetBundle(ulong bundleId, bool cacheLoadedAsset, Action<AssetBundleReference> completeHandle = null)
        {
            LoadAssetBundle(bundleId, 0, cacheLoadedAsset, completeHandle);
        }

        public void LoadAssetBundle(ulong bundleId, int tag, bool cacheLoadedAsset, Action<AssetBundleReference> completeHandle = null)
        {
            if (m_Context != null && m_Context.enable)
            {
                AMDebug.LogFormat("[AssetManage]ContextAssetLoader Start load asset bundle {0}.", bundleId.ToString());
                //这里暂时使用匿名函数。
                AssetBundleLoader loader = AssetManager.Instance.LoadAssetBundle(bundleId, tag, cacheLoadedAsset, completeHandle);
                if (loader!=null)
                {
                    loader.onBeforeComplete += OnAssetBundleBeforeComplete;
                    m_AssetBundleLoaders.Add(loader);
                    m_AssetBundleCompleteHandles[loader] = completeHandle;
                }
            }
            else
            {
                AMDebug.LogFormat("[ContextAssetLoader] Can't load asset bundle {0}.The context is disable", bundleId);
            }
        }
        #endregion

        #region Assets

        public void LoadAssets(ICollection<string> assets, Action<Dictionary<string, AssetReference>> callback)
        {
            Dictionary<string, AssetReference> assetReferences = new Dictionary<string, AssetReference>();
            LoadAssets(assets, assetReferences, callback);
        }

        public void LoadAssets(ICollection<string> assets, Dictionary<string, AssetReference> assetReferences, Action<Dictionary<string, AssetReference>> callback)
        {
            if (m_Context != null && m_Context.enable)
            {
                AMDebug.Log("[AssetManage]ContextAssetLoader Start load assets.");

				int needCount = assets.Count;
                int loadCount = 0;
                bool checkAll = false;

                foreach (var asset in assets)
                {
                    if (!string.IsNullOrEmpty(asset))
                    {
                        ++loadCount;
                        LoadAsset(asset, (assetReference) =>
                        {
                            if (assetReference != null && !assetReference.IsEmpty())
                            {
                                if (assetReferences != null)
                                {
                                    assetReferences[asset] = assetReference;
                                }
                            }
                            else
                            {
                                AMDebug.LogErrorFormat("[AssetManage]LoadAssets can't load {0}", asset);
                            }
                            //all finished
                            --needCount;
                            if (--loadCount <= 0)
                            {
                                if (checkAll)
                                {
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
                    if (callback!=null)
                    {
                        callback(assetReferences);
                    }
                }
            }
            else
            {
                AMDebug.Log("[AssetManage]ContextAssetLoader Can't load assets .The context is disable");
            }
        }

        #endregion

        #region Event
        void OnAssetBeforeComplete(AssetLoader loader)
        {
            AMDebug.Log("[AssetManage]ContextAssetLoader OnAssetBeforeComplete.");
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
            AMDebug.Log("[AssetManage]ContextAssetLoader OnAssetBundleBeforeComplete.");
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
            AMDebug.Log("[AssetManage]ContextAssetLoader OnContextDisable.");
            if (s_CacheContextAssetLoaders.ContainsKey(m_Context))
            {
                s_CacheContextAssetLoaders.Remove(m_Context);
            }

            Clean();

            s_ContextAssetLoaderPool.Release(this);
        }

        #endregion

        public void Clean()
        {
            AMDebug.Log("[AssetManage]ContextAssetLoader Clean.");
            if (m_Context != null)
            {
                m_Context.onDisable -= OnContextDisable;
                m_Context = null;
            }

            ClearLoaders();
        }

        public void ClearLoaders()
        {
            AMDebug.Log("[AssetManage]ContextAssetLoader ClearLoaders.");
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
