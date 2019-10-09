using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class LoaderManager
    {
        AssetManager m_AssetManager;

        //int m_MaxActiveLoader = 100;
        //List<Loader> m_ActiveLoaders = ListPool<Loader>.Get();
        //List<int> m_FinishedIndexs = ListPool<int>.Get();
        //Stack<Loader> m_PrepareLoaders = StackPool<Loader>.Get();

        public LoaderManager(AssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }

        public AssetLoader CreateAssetAsyncLoader(string path)
        { 
            AssetLoader loader = null;
            AssetInfo info = null;

#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            info = m_AssetManager.infoManager.FindAssetInfo(path);
            //can't find asset info
            if (info == null)
            {
                info = new AssetInfo();
                info.fullName = path;
            }

            loader = LoaderPool.AssetAsyncLoaderPool.Get();// new AssetAsyncLoader();
#else
            loader = new AssetEditorLoader();
            info = new AssetInfo();
            info.fullName = path;
#endif
            loader.info = info;
            loader.assetManager = m_AssetManager;
            return loader;
        }

        public AssetBundleLoader CreateAssetBundleAsyncLoader(string path)
        {
            AssetBundleLoader loader = null;
            AssetBundleInfo info = null;
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            info = m_AssetManager.infoManager.FindAssetBundleInfo(path);
            if (info == null)
            {
                Debug.LogErrorFormat("Can't find asset bundle info {0}", path);
                return null;
            }
    #if ASSET_BUNDLE_REMOTE
            if (m_AssetManager.infoManager.NeedDownload(path,info.hash))
            {
                loader = LoaderPool.AssetBundleNetworkLoaderPool.Get();
                loader.onBeforeComplete += (_loader) =>
                {
                    m_AssetManager.infoManager.UpdateAssetBundleVersion(path, info.hash);
                };
            }
            else
            {
                loader = LoaderPool.AssetBundleAsyncLoaderPool.Get();
            }
    #else
            loader = LoaderPool.AssetBundleAsyncLoaderPool.Get();
    #endif
#else
            loader = new AssetBundleEmptyLoader();
            //just for message
            info = new AssetBundleInfo();
            info.fullName = path;
#endif

            loader.info = info;
            loader.assetManager = m_AssetManager;
            return loader;
        }

        public AssetSyncLoader CreateAssetSyncLoader(string path)
        {
            AssetSyncLoader loader = null;
            AssetInfo info = null;

#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            info = m_AssetManager.infoManager.FindAssetInfo(path);
            //can't find asset info
            if (info == null)
            {
                info = new AssetInfo();
                info.fullName = path;
            }

            loader = new AssetSyncLoader();
#else
            loader = new AssetSyncLoader();
            info = new AssetInfo();
            info.fullName = path;
#endif
            loader.info = info;
            loader.assetManager = m_AssetManager;
            return loader;
        }

        public AssetBundleSyncLoader CreateAssetBundleSyncLoader(string path)
        {
            AssetBundleSyncLoader loader = null;

#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            AssetBundleInfo info = null;
            info = m_AssetManager.infoManager.FindAssetBundleInfo(path);
            if (info != null)
            {
                loader = new AssetBundleSyncLoader();
                loader.info = info;
                loader.assetManager = m_AssetManager;
            }
            else
            {
                Debug.LogErrorFormat("Can't find asset bundle info {0}", path);
            }
#endif

            return loader;
        }

        public void Clean()
        {
            //ListPool<Loader>.Release(m_ActiveLoaders);
            //ListPool<int>.Release(m_FinishedIndexs);
            //StackPool<Loader>.Release(m_PrepareLoaders);
        }

        public void ActiveLoader(Loader loader)
        {
            loader.Start();
            //if (m_ActiveLoaders.Count < m_MaxActiveLoader)
            //{
            //    m_ActiveLoaders.Add(loader);
            //    loader.Start();
            //}
            //else
            //{
            //    m_PrepareLoaders.Push(loader);
            //}
        }

        //public void Update()
        //{
        //    //check request 
        //    Tick();
        //}

        //public void Tick()
        //{
        //    m_FinishedIndexs.Clear();

        //    try
        //    {
        //        Loader loader = null;
        //        for (int i = 0, l = m_ActiveLoaders.Count; i < l; ++i)
        //        {
        //            loader = m_ActiveLoaders[i];
        //            loader.Update();

        //            if (loader.isDone)
        //            {
        //                m_FinishedIndexs.Add(i);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError(e);
        //    }

        //    CheckAndStartRequests();
        //}

        //protected void CheckAndStartRequests()
        //{
        //    if (m_PrepareLoaders.Count >= m_FinishedIndexs.Count)
        //    {
        //        //替换激活的请求
        //        Loader loader = null;
        //        for (int i = m_FinishedIndexs.Count - 1; i >= 0; --i)
        //        {
        //            loader = m_PrepareLoaders.Pop();
        //            m_ActiveLoaders[m_FinishedIndexs[i]] = loader;
        //            loader.Start();
        //        }
        //    }
        //    else
        //    {
        //        Loader loader = null;
        //        int i = 0;
        //        //替换一部分
        //        for (int l = m_PrepareLoaders.Count; i < l; ++i)
        //        {
        //            loader = m_PrepareLoaders.Pop();
        //            m_ActiveLoaders[m_FinishedIndexs[i]] = loader;
        //            loader.Start();
        //        }
        //        //移除剩余的
        //        for (int j = m_FinishedIndexs.Count - 1; j >= i; --j)
        //        {
        //            m_ActiveLoaders.RemoveAt(m_FinishedIndexs[j]);
        //        }
        //    }
        //}
    }
}
