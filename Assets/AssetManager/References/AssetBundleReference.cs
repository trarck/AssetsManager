using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetBundleReference:BaseReference
    {
        HashSet<AssetBundleReference> m_Dependencies=HashSetPool<AssetBundleReference>.Get();

        public AssetBundle assetBundle { get; set; }

        public string assetBundleName { get; set; }

        public AssetBundleReference(AssetBundle assetBundle,string assetBundleName)
        {
            this.assetBundle = assetBundle;
            this.assetBundleName = assetBundleName;
        }

        public void AddDependency(AssetBundleReference dependency)
        {
            if(dependency!=null && m_Dependencies.Add(dependency))
            {
                dependency.Retain();
            }
        }
        
        public override void Dispose()
        {

            UnloadBundle();
            ReleaseDependencies();

            base.Dispose();
        }

        void UnloadBundle()
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(false);
                assetBundle = null;
            }
        }

        void ReleaseDependencies()
        {
            var iter = m_Dependencies.GetEnumerator();
            while (iter.MoveNext())
            {
                iter.Current.Release();
            }
            HashSetPool<AssetBundleReference>.Release(m_Dependencies);
            m_Dependencies = null;
        }

        public override void Reset()
        {
            base.Reset();
            m_Dependencies= HashSetPool<AssetBundleReference>.Get();
        }       
    }
}
