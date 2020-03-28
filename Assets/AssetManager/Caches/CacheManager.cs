using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YH.AssetManage
{
    [Serializable]
    public class CacheItem
    {
        public string name;
        public string hash;
        //更新时间。秒
        public long lastTime;
    }

    [Serializable]
    public class CacheInfo
    {
        public List<CacheItem> items;
    }

    public class CacheManager
    {
        Dictionary<string, CacheItem> m_CacheItems = null;
        string m_CacheInfoFile = "CacheInfo.json";
        bool m_Dirty = false;
        float m_DelayDuration = 2.0f;
        float m_Elapsed = 0;
        int m_Expiration=3600*24*120;//120天

        public bool IsCached(string name,string hash)
        {
            CacheItem cacheItem = null;
            if(m_CacheItems!=null && m_CacheItems.TryGetValue(name,out cacheItem))
            {
                if (cacheItem.hash == hash)
                {
                    if ((long)(DateTime.Now.Ticks * 0.0000001d) - cacheItem.lastTime <= m_Expiration)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Update(float dt)
        {
            if (m_Dirty)
            {
                m_Elapsed += dt;
                if (m_Elapsed >= m_DelayDuration)
                {
                    SaveCacheInfo();
                }
            }
        }

        public void FreshCacheItems(CacheInfo cacheInfo)
        {
            if (cacheInfo != null)
            {
                if (m_CacheItems != null)
                {
                    m_CacheItems.Clear();
                }
                else
                {
                    m_CacheItems = new Dictionary<string, CacheItem>();
                }

                foreach (var cacheItem in cacheInfo.items)
                {
                    m_CacheItems.Add(cacheItem.name, cacheItem);
                }
            }
        }

        public void AppendCacheItems(CacheInfo cacheInfo)
        {
            if (cacheInfo != null)
            {
                if (m_CacheItems == null)
                {
                    m_CacheItems = new Dictionary<string, CacheItem>();
                }

                foreach (var cacheItem in cacheInfo.items)
                {
                    m_CacheItems.Add(cacheItem.name, cacheItem);
                }
            }
        }

        public void LoadCacheInfo()
        {
            if (File.Exists(cacheInfoFile))
            {
                string content = File.ReadAllText(cacheInfoFile);
                CacheInfo cacheInfo = JsonUtility.FromJson<CacheInfo>(content);
                FreshCacheItems(cacheInfo);
            }
        }

        public void SaveCacheInfo()
        {
            if (m_CacheItems != null && m_Dirty)
            {
                CacheInfo cacheInfo = new CacheInfo();
                cacheInfo.items = new List<CacheItem>(m_CacheItems.Values);
                string content = JsonUtility.ToJson(cacheInfo);
                File.WriteAllText(cacheInfoFile, content);
                m_Dirty = false;
            }
        }

        public void UpdateCacheItem(string key, string hash)
        {
            m_Dirty = true;
            CacheItem cacheItem;
            if(!m_CacheItems.TryGetValue(key,out cacheItem))
            {
                cacheItem = new CacheItem();
                cacheItem.name = key;
            }
            //转换成秒
            cacheItem.lastTime = (long)(DateTime.Now.Ticks * 0.0000001d);
            cacheItem.hash= hash;
        }

        public string cacheInfoFile
        {
            get
            {
                return m_CacheInfoFile;
            }
            set
            {
                m_CacheInfoFile = value;
            }
        }

        public int expiration
        {
            get
            {
                return m_Expiration;
            }
            set
            {
                m_Expiration = value;
            }
        }

    }
}
