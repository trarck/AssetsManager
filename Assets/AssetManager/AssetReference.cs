using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetReference
    {
        //默认为1，相当于默认执行Retain
        int m_RefCount=1;
        List<WeakReference> m_Owners=ListPool<WeakReference>.Get();

        public AssetBundleReference assetBundleReference { get; set; }

        public string assetPath { get; set; }

        public Object asset { get; set; }

        public delegate void DisposeHandle(AssetReference abr);

        public event DisposeHandle onDispose;

        public AssetReference(AssetBundleReference assetBundleReference, string assetPath)
        {
            this.assetBundleReference = assetBundleReference;
            this.assetPath = assetPath;
        }
        
        public void Retain()
        {
            ++m_RefCount;
            if (assetBundleReference != null)
            {
                assetBundleReference.Retain();
            }
        }

        public void Release()
        {
            --m_RefCount;
            if (assetBundleReference != null)
            {
                assetBundleReference.Release();
            }
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

            if (assetBundleReference != null)
            {
                assetBundleReference.Retain(owner);
            }
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

            if (assetBundleReference != null)
            {
                assetBundleReference.Release(owner);
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

            if (assetBundleReference == null)
            {
                Resources.UnloadAsset(asset);
            }

            ListPool<WeakReference>.Release(m_Owners);
            m_Owners = null;
        }


        public void Reset()
        {
            m_RefCount = 1;
            m_Owners= ListPool<WeakReference>.Get();
        }      
    }
}
