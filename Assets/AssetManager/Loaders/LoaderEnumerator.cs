using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class LoaderEnumerator : IEnumerator, IDisposable
    {
        protected Loader m_Loader;

        public object Current
        {
            get
            {
                return null;
            }
        }

        public LoaderEnumerator(Loader loader)
        {
            m_Loader = loader;
        }

        public bool MoveNext()
        {
            return m_Loader != null && !m_Loader.isDone;
        }

        public void Reset()
        {

        }

        void IDisposable.Dispose()
        {
#if ASSETMANAGER_LOG
            Debug.Log("#### Dispose " + Time.frameCount);
#endif
            if (m_Loader != null)
            {
                m_Loader.Clean();
            }
        }
    }

    public class AssetLoaderEnumerator: LoaderEnumerator
    {
        public AssetLoaderEnumerator(AssetLoader loader):base(loader)
        {

        }

        public AssetReference assetReference
        {
            get
            {
                AssetLoader loader = m_Loader as AssetLoader;
                if (loader!=null)
                {
                    return loader.result;
                }
                return null;
            }
        }
    }

    public class BundleLoaderEnumerator : LoaderEnumerator
    {
        public BundleLoaderEnumerator(AssetBundleLoader loader) : base(loader)
        {

        }

        public AssetBundleReference bundleReference
        {
            get
            {
                AssetBundleLoader loader = m_Loader as AssetBundleLoader;
                if (loader != null)
                {
                    return loader.result;
                }
                return null;
            }
        }
    }
}
