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
            AssetInfo info = m_AssetManager.infoManager.FindAssetInfo(path);
            if (info != null)
            {
                AssetLoader loader = new AssetLoader();
                loader.info = info;
                return loader;
            }

            return null;
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
