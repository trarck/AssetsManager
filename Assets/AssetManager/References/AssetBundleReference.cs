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

        public delegate void DisposeHandle(AssetBundleReference abr);

        public event DisposeHandle onDispose;

        public AssetBundleReference(AssetBundle assetBundle,string assetBundleName)
        {
            this.assetBundle = assetBundle;
            this.name = assetBundleName;
        }

        public void AddDependency(AssetBundleReference dependency)
        {
            if(dependency!=null && m_Dependencies.Add(dependency))
            {
                dependency.Retain();
            }
        }

        public void AddDependencies(HashSet<AssetBundleReference> dependencies)
        {
            if(dependencies!=null && dependencies.Count > 0)
            {
                HashSet<AssetBundleReference>.Enumerator iter = dependencies.GetEnumerator();
                while (iter.MoveNext())
                {
                    AddDependency(iter.Current);
                }
            }
        }

        public override void Dispose()
        {
            if (onDispose != null)
            {
                onDispose(this);
                onDispose = null;
            }

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

        public override void AddTag(string tag)
        {
            base.AddTag(tag);
            if (m_Dependencies != null)
            {
                var iter = m_Dependencies.GetEnumerator();
                while (iter.MoveNext())
                {
                    iter.Current.AddTag(tag);
                }
            }
        }

        public override void AddTags(string[] tags)
        {
            base.AddTags(tags);
            if (m_Dependencies != null)
            {
                var iter = m_Dependencies.GetEnumerator();
                while (iter.MoveNext())
                {
                    iter.Current.AddTags(tags);
                }
            }
        }

        public override void AddTags(ICollection<string> tags)
        {
            base.AddTags(tags);
            if (m_Dependencies != null)
            {
                var iter = m_Dependencies.GetEnumerator();
                while (iter.MoveNext())
                {
                    iter.Current.AddTags(tags);
                }
            }
        }

        public override void RemoveTag(string tag)
        {
            if (m_Dependencies != null)
            {
                var iter = m_Dependencies.GetEnumerator();
                while (iter.MoveNext())
                {
                    iter.Current.AddTag(tag);
                }
            }

            base.RemoveTag(tag);
        }

        public override void RemoveTags(string[] tags)
        {
            if (m_Dependencies != null)
            {
                var iter = m_Dependencies.GetEnumerator();
                while (iter.MoveNext())
                {
                    iter.Current.AddTags(tags);
                }
            }
            base.RemoveTags(tags);
        }

        public override void RemoveTags(ICollection<string> tags)
        {
            if(m_Dependencies != null)
            {
                var iter = m_Dependencies.GetEnumerator();
                while (iter.MoveNext())
                {
                    iter.Current.AddTags(tags);
                }
            }
            base.RemoveTags(tags);
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
