using System;
using System.Collections;
using System.Collections.Generic;

namespace YH.AssetManager
{
    public abstract class Loader:IDisposable
    {
        //public Action<AssetBundleReference> onComplete;

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
        protected bool m_Standalone = false;
        protected bool m_AutoRelease = true;

        HashSet<string> m_ParamTags = null;

        public abstract bool isDone { get; }

        public bool autoRelease { get { return m_AutoRelease; } set { m_AutoRelease = value; } }

        public HashSet<string> paramTags
        {
            get
            {
                if (m_ParamTags == null)
                {
                    m_ParamTags = new HashSet<string>();
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
            m_Standalone = false;
            m_AutoRelease = true;
            m_ParamTags.Clear();
            assetManager = null;
        }

        #region IDisposable Support
        protected bool m_Disposed = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    //释放托管状态(托管对象)。
                    if (m_ParamTags != null)
                    {
                        m_ParamTags = null;
                    }

                    if (assetManager != null)
                    {
                        assetManager = null;
                    }
                }

                // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // 将大型字段设置为 null。

                m_Disposed = true;
            }
        }

        ~Loader()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion

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

        public void AddParamTag(string tag)
        {
            paramTags.Add(tag);
        }

        public bool standalone
        {
            get
            {
                return m_Standalone;
            }

            set
            {
                m_Standalone = value;
            }
        }
    }
}
