using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public abstract class BaseReference
    {
        protected int m_RefCount=0;
        //是否要缓存。
        //资源都是要缓存
        protected bool m_IsCached = false;

        List<WeakReference> m_Owners=ListPool<WeakReference>.Get();

        HashSet<int> m_Tags = HashSetPool<int>.Get();

        public string name { get; set; }

        public virtual void Retain()
        {
            ++m_RefCount;
#if ASSETMANAGER_LOG
            Debug.LogFormat("[{0}].Retain refCount={1},name={2},{3}" ,this,m_RefCount,name, Time.frameCount);
#endif
        }

        public virtual void Release()
        {
            --m_RefCount;
#if ASSETMANAGER_LOG
            Debug.LogFormat("[{0}].Release refCount={1},name={2},{3}", this, m_RefCount, name, Time.frameCount);
#endif
            //check sub overflow
            if (m_RefCount < 0)
            {
                m_RefCount = 0;
            }

            CheckRefCount();
        }

        public virtual void Retain(Object owner)
        {
#if ASSETMANAGER_LOG
            Debug.LogFormat("[{0}].Retain refCount={1},name={2},owner={3},{4}", this, m_RefCount, name, owner, Time.frameCount);
#endif
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

        public virtual void Release(Object owner)
        {
#if ASSETMANAGER_LOG
            Debug.LogFormat("[{0}].Release refCount={1},name={2},owner={3},{4}", this, m_RefCount, name, owner, Time.frameCount);
#endif
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

        public virtual void Monitor(GameObject gameObject)
        {

        }

        public virtual void Monitor(GameObject gameObject, int tag)
        {

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
            return m_RefCount==(isCache?1:0) && GetOwnersRefCount() == 0;
        }

        public virtual void Dispose(bool disposing = false)
        {
            ListPool<WeakReference>.Release(m_Owners);
            m_Owners = null;
            HashSetPool<int>.Release(m_Tags);
        }

        public virtual void Reset()
        {
            m_RefCount = 0;
            m_IsCached = false;
            m_Owners.Clear();
            m_Tags.Clear();
            name = null;
        }

        public virtual void AddTag(int tag)
        {
            if (tag>0)
            {
                m_Tags.Add(tag);
            }
        }

        public virtual void AddTags(int[] tags)
        {
            if (tags != null)
            {
                for (int i = 0, l = tags.Length; i < l; ++i)
                {
                    m_Tags.Add(tags[i]);
                }
            }
        }

        public virtual void AddTags(ICollection<int> tags)
        {
            if (tags != null)
            {
                var iter = tags.GetEnumerator();
                while (iter.MoveNext())
                {
                    m_Tags.Add(iter.Current);
                }
            }
        }

        public virtual void RemoveTag(int tag)
        {
            if (tag>0)
            {
                m_Tags.Remove(tag);
            }
        }

        public virtual void RemoveTags(int[] tags)
        {
            if (tags != null)
            {
                for (int i = 0, l = tags.Length; i < l; ++i)
                {
                    m_Tags.Remove(tags[i]);
                }
            }
        }

        public virtual void RemoveTags(ICollection<int> tags)
        {
            if (tags != null)
            {
                var iter = tags.GetEnumerator();
                while (iter.MoveNext())
                {
                    m_Tags.Remove(iter.Current);
                }
            }
        }

        public bool HaveTag(int tag)
        {
            return m_Tags.Contains(tag);
        }

        public int tagCount
        {
            get
            {
                return m_Tags.Count;
            }
        }

        public bool isCache
        {
            get
            {
                return m_IsCached;
            }

            set
            {
                m_IsCached = value;
            }
        }

        public void Cache()
        {
            if (!isCache)
            {
                isCache = true;
                Retain();
            }
        }

        public void UnCache()
        {
            if (isCache)
            {
                isCache = false;
                Release();
            }
        }
    }
}
