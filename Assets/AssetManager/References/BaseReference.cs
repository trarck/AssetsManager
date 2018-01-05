using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public abstract class BaseReference
    {
        //默认为1，相当于默认执行Retain
        int m_RefCount=1;
        List<WeakReference> m_Owners=ListPool<WeakReference>.Get();

        HashSet<string> m_Tags = HashSetPool<string>.Get();

        public string name { get; set; }

        public virtual void Retain()
        {
            ++m_RefCount;
        }

        public virtual void Release()
        {
            --m_RefCount;
            //check sub overflow
            if (m_RefCount < 0)
            {
                m_RefCount = 0;
            }

            CheckRefCount();
        }

        public virtual void Retain(Object owner)
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

        public virtual void Release(Object owner)
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

        public virtual void Monitor(GameObject gameObject)
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
            return m_RefCount == 1 && GetOwnersRefCount() == 0;
        }

        public virtual void Dispose()
        {
            ListPool<WeakReference>.Release(m_Owners);
            m_Owners = null;
            HashSetPool<string>.Release(m_Tags);
        }

        public virtual void Reset()
        {
            m_RefCount = 1;
            m_Owners= ListPool<WeakReference>.Get();
            m_Tags = HashSetPool<string>.Get();
        }

        public void AddTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                m_Tags.Add(tag);
            }
        }

        public void AddTags(string[] tags)
        {
            if (tags != null)
            {
                for (int i = 0, l = tags.Length; i < l; ++i)
                {
                    m_Tags.Add(tags[i]);
                }
            }
        }

        public void AddTags(ICollection<string> tags)
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

        public void RemoveTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                m_Tags.Remove(tag);
            }
        }

        public void RemoveTags(string[] tags)
        {
            if (tags != null)
            {
                for (int i = 0, l = tags.Length; i < l; ++i)
                {
                    m_Tags.Remove(tags[i]);
                }
            }
        }

        public bool HaveTag(string tag)
        {
            return m_Tags.Contains(tag);
        }
    }
}
