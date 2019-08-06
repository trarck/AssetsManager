using System;
using System.Collections;
using System.Collections.Generic;

namespace YH.AssetManager
{
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
        protected bool m_CacheLoadedAsset = false;
        protected bool m_AutoRelease = true;
        //是否在加载后自动断开和AssetBundle的联系。
        protected bool m_AutoReleaseBundle = true;

        HashSet<int> m_ParamTags = null;

        public abstract bool isDone { get; }

        public bool autoRelease { get { return m_AutoRelease; } set { m_AutoRelease = value; } }
        public bool autoReleaseBundle { get { return m_AutoReleaseBundle; } set { m_AutoReleaseBundle = value; } }

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

        public virtual void Start()
        {

        }

        public virtual void Update()
        {

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
            m_CacheLoadedAsset = false;
            m_AutoRelease = true;
            m_ParamTags.Clear();
            assetManager = null;
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

        public bool cacheLoadedAsset
        {
            get
            {
                return m_CacheLoadedAsset;
            }

            set
            {
                m_CacheLoadedAsset = value;
            }
        }
    }
}
