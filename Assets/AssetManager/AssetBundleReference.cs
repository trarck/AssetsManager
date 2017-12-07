using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetBundleReference
    {
        //默认为1，相当于默认执行Retain
        int m_RefCount=1;
        List<WeakReference> m_Owners=ListPool<WeakReference>.Get();
        HashSet<AssetBundleReference> m_Dependencies=HashSetPool<AssetBundleReference>.Get();
        //List<string> m_Dependents = ListPool<string>.Get();

        List<string> m_Tags = ListPool<string>.Get();

        public AssetBundle assetBundle { get; set; }
        public int level { get; set; }

        public delegate void DisposeHandle(AssetBundleReference abr);

        public event DisposeHandle onDispose;

        public void AddDependency(AssetBundleReference dependency)
        {
            if(dependency!=null && m_Dependencies.Add(dependency))
            {
                dependency.Retain();
            }
        }

        public void Retain()
        {
            ++m_RefCount;
        }

        public void Release()
        {
            --m_RefCount;
            CheckRefCount();
        }

        public void Retain(Object owner)
        {
            if (owner == null)
            {
                throw new Exception("AssetBundleReference Retain a null Object");
            }

            for(int i=0,l=m_Owners.Count;i< l; ++i)
            {
                if (owner.Equals(m_Owners[i].Target))
                {
                    return;
                }
            }
            WeakReference weakRef = new WeakReference(owner);
            m_Owners.Add(weakRef);
        }

        public void Release(Object owner)
        {
            for (int i = 0, l = m_Owners.Count; i < l; ++i)
            {
                if (owner.Equals(m_Owners[i].Target))
                {
                    m_Owners.RemoveAt(i);
                    break;
                }
            }

            CheckRefCount();
        }

        void CheckRefCount()
        {
            //if no ref remove id
            if(m_RefCount==0 && GetOwnersRefCount()== 0)
            {
                Dispose();
            }
        }

        int GetOwnersRefCount()
        {
            for(int i = m_Owners.Count-1; i >= 0; --i)
            {
                Object o = (Object)m_Owners[i].Target;
                if (!o)
                {
                    m_Owners.RemoveAt(i);
                }
            }
            return m_Owners.Count;
        }

        public bool isUnused()
        {
            return m_RefCount == 1 && GetOwnersRefCount() == 0;
        }

        public void Dispose()
        {
            if (onDispose != null)
            {
                onDispose(this);
                onDispose = null;
            }

            UnloadBundle();
            ReleaseDependencies();
            ListPool<WeakReference>.Release(m_Owners);
            m_Owners = null;
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

        public void Reset()
        {
            m_RefCount = 1;
            m_Owners= ListPool<WeakReference>.Get();
            m_Dependencies= HashSetPool<AssetBundleReference>.Get();
            m_Tags = ListPool<string>.Get();
        }
    }
}
