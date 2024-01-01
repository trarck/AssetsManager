using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    /// <summary>
    /// Loader的生命周期由管理器来管理，使用池循环使用。
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


        HashSet<int> m_ParamTags = null;

		//引用计数。通常情况下用不到。
		protected int m_RefCount = 0;

		//正在加载的数量
		protected int m_LoadingRefCount = 0;
		//中止标识
		protected bool m_Aborted = false;

		public abstract bool isDone { get; }

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

		public ILoaderManager loaderManager { get; set; }

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
            AMDebug.LogFormat("[AssetManage]({0}#{1}).Retain refCount={2}", this,GetHashCode(), m_RefCount);
		}

		public virtual void Release()
		{
			--m_RefCount;
            AMDebug.LogFormat("[AssetManage]({0}#{1}).Release refCount={2}", this, GetHashCode(), m_RefCount);
		}

		/// <summary>
		/// 检查是否有引用
		/// </summary>
		public bool isEmptyRef
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
			//m_RefCount = 0;
			m_LoadingRefCount = 0;
            m_Aborted = false;
            loaderManager = null;
        }

        public void IncreaseLoadingCount()
        {
            ++m_LoadingRefCount;
        }

        public void DecreaseLoadingCount()
        {
            if (--m_LoadingRefCount <= 0)
            {
				m_LoadingRefCount = 0;
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
