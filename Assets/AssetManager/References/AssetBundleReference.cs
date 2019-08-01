using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetBundleReference : BaseReference
    {
        HashSet<AssetBundleReference> m_Dependencies = HashSetPool<AssetBundleReference>.Get();

        public AssetBundle assetBundle { get; set; }

        public delegate void DisposeHandle(AssetBundleReference abr);

        public event DisposeHandle onDispose;

        public AssetBundleReference(AssetBundle assetBundle, string assetBundleName)
        {
            this.assetBundle = assetBundle;
            this.name = assetBundleName;
        }

        public void AddDependency(AssetBundleReference dependency)
        {
            if (dependency != null && m_Dependencies.Add(dependency))
            {
                dependency.Retain();
            }
        }

        public void AddDependencies(HashSet<AssetBundleReference> dependencies)
        {
            if (dependencies != null && dependencies.Count > 0)
            {
                HashSet<AssetBundleReference>.Enumerator iter = dependencies.GetEnumerator();
                while (iter.MoveNext())
                {
                    AddDependency(iter.Current);
                }
            }
        }

        public override void Dispose(bool disposing = false)
        {
#if ASSETMANAGER_LOG
            Debug.Log("Bundle dispose " + name + "," + Time.frameCount);
#endif

            if (onDispose != null)
            {
                onDispose(this);
                onDispose = null;
            }

            UnloadBundle(disposing);
            ReleaseDependencies();

            base.Dispose(disposing);
        }

        void UnloadBundle(bool unloadAllLoadedObjects=false)
        {
            if (assetBundle != null)
            {
#if ASSETMANAGER_LOG
                Debug.Log("AssetBundle unload " + name);
#endif
                assetBundle.Unload(unloadAllLoadedObjects);
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
            m_Dependencies.Clear();
            assetBundle = null;
            onDispose = null;
            base.Reset();
        }

        public HashSet<AssetBundleReference> dependencies
        {
            get
            {
                return m_Dependencies;
            }
        }
    }
}
