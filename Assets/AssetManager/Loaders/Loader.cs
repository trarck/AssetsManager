using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    /// <summary>
    /// Loader由管理器来管理，不需要自己的Dispose.
    /// 加载同一个资源的loader同时只能存在一个。
    /// </summary>
    public abstract class Loader
    {
        public enum State
        {
            Idle,
            Inited,
            Loading,
            Loaded,
            Completed,
            Error
        }

        protected State m_State = State.Idle;
        protected bool m_ForceDone = false;
        //是否缓存加载后的资源。
        protected bool m_CacheResult = false;
        //是否在加载后自动断开和AssetBundle的联系。
        protected bool m_AutoReleaseBundle = true;

        HashSet<int> m_ParamTags = null;
        
        protected int m_RefCount = 0;

        public abstract bool isDone { get; }

        public bool autoReleaseBundle { get { return m_AutoReleaseBundle; } set { m_AutoReleaseBundle = value; } }
        //正在加载时请求的数量
        protected int m_LoadingRequestCount = 0;
        protected bool m_Aborted = false;

        public HashSet<int> paramTags
        {
            get
            {
                if (m_ParamTags == null)
                {
                    m_ParamTags = new HashSet<int>();
                }
                return m_ParamTags;
            }
            set
            {
                m_ParamTags = value;
            }
        }

        public AssetManager assetManager { get; set; }

        public bool forceDone
        {
            get
            {
                return m_ForceDone;
            }
            set
            {
                m_ForceDone = value;
                state = State.Completed;
            }
        }

        public State state
        {
            get
            {
                return m_State;

            }
            set
            {
                m_State = value;
            }
        }

        
        public virtual void Retain()
        {
            ++m_RefCount;
#if ASSETMANAGER_LOG_ON
            Debug.LogFormat("[AssetManage]({0}#{1}).Retain refCount={2}---{3}", this,GetHashCode(), m_RefCount, Time.frameCount);
#endif
        }

        public virtual void Release()
        {
            --m_RefCount;
#if ASSETMANAGER_LOG_ON
            Debug.LogFormat("[AssetManage]({0}#{1}).Release refCount={2}---{3}", this, GetHashCode(), m_RefCount, Time.frameCount);
#endif
            //check sub overflow
            if (m_RefCount < 0)
            {
                m_RefCount = 0;
            }
        }

        /// <summary>
        /// 检查是否有引用
        /// </summary>
        public bool emptyRef
        {
            get
            {
                return m_RefCount <= 0;
            }
        }

        public virtual void Start()
        {

        }

        public virtual void Abort()
        {
            m_Aborted = true;
        }

        public virtual void Complete()
        {
            state = State.Completed;
        }

        public virtual void Error()
        {
            state = State.Error;
        }

        public virtual void Clean()
        {
            state = State.Idle;
            m_ForceDone = false;
            m_CacheResult = false;
            if (m_ParamTags != null)
            {
                m_ParamTags.Clear();
            }
            m_LoadingRequestCount = 0;
            m_Aborted = false;
            assetManager = null;
        }

        public void IncreaseLoadingRequest()
        {
            ++m_LoadingRequestCount;
        }

        public void DecreaseLoadingRequest()
        {
            if (--m_LoadingRequestCount <= 0)
            {
                Abort();
            }
        }

        protected bool isFinishedState
        {
            get
            {
                return m_State == State.Completed || m_State == State.Error;
            }
        }

        public virtual bool haveError
        {
            get
            {
                return m_State == State.Error;
            }
        }

        public void AddParamTag(int tag)
        {
            paramTags.Add(tag);
        }

		/// <summary>
		/// 防止设置过true被false覆盖
		/// </summary>
		/// <param name="val"></param>
		public void SetCacheResult(bool val)
		{
			if (!m_CacheResult && val)
			{
				m_CacheResult = val;
			}
		}

        public bool cacheResult
        {
            get
            {
                return m_CacheResult;
            }

            set
            {
                m_CacheResult = value;
            }
        }

    }
}
