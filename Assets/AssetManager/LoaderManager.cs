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

#if UNITY_EDITOR && ASSET_EDITOR_LOADER
            loader = new AssetEditorLoader();
            info = new AssetInfo();
            info.name = path;
#else
            loader = new AssetLoader();
            info = m_AssetManager.infoManager.FindAssetInfo(path);
            //can't find asset info
            if (info == null)
            {
                info = new AssetInfo();
                info.name = path;
            }
#endif
            loader.info = info;
            return loader;
        }

        public AssetBundleLoader CreateAssetBundleLoader(string path)
        {
            AssetBundleInfo info = m_AssetManager.infoManager.FindAssetBundleInfo(path);
            if (info != null)
            {
                AssetBundleLoader loader = new AssetBundleLoader();
                loader.info = info;
                return loader;
            }

            return null;
        }
        
    }
}
