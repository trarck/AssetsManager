using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class LoaderManager
    {
        AssetManager m_AssetManager;

        public LoaderManager(AssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }

        public AssetLoader CreateAssetLoader(string path)
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

            loader = new AssetLoader();
#else
            loader = new AssetEditorLoader();
            info = new AssetInfo();
            info.fullName = path;
#endif
            loader.info = info;
            loader.assetManager = m_AssetManager;
            return loader;
        }

        public AssetBundleLoader CreateAssetBundleLoader(string path)
        {
            AssetBundleLoader loader = null;
            AssetBundleInfo info = null;
#if !UNITY_EDITOR || ASSET_BUNDLE_LOADER
            loader = new AssetBundleLoader();

            info = m_AssetManager.infoManager.FindAssetBundleInfo(path);
            if (info == null)
            {
                Debug.LogErrorFormat("Can't find asset bundle info {0}", path);
            }
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
    }
}
