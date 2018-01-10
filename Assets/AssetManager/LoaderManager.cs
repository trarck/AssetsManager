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
            AssetBundleInfo info = m_AssetManager.infoManager.FindAssetBundleInfo(path);
            if (info != null)
            {
                AssetBundleLoader loader = new AssetBundleLoader();
                loader.info = info;
                loader.assetManager = m_AssetManager;
                return loader;
            }

            return null;
        }
        
    }
}
