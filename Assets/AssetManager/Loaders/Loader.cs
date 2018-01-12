using System;
using System.Collections;
using System.Collections.Generic;

namespace YH.AssetManager
{
    public abstract class Loader : DataEnumerator
    {
        //public Action<AssetBundleReference> onComplete;

        public enum State
        {
            Idle,
            Inited,
            Loading,
            Completed,
            Error
        }

        protected State m_State = State.Idle;
        protected bool m_ForceDone = false;
        protected bool m_Standalone = false;

        HashSet<string> m_ParamTags = HashSetPool<string>.Get();

        public HashSet<string> paramTags
        {
            get
            {
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

        public virtual void Complete()
        {

        }

        public virtual void Error()
        {

        }

        
        public virtual void Clean()
        {
            HashSetPool<string>.Release(m_ParamTags);
            m_ParamTags = null;
            assetManager = null;
        }
        //public virtual AssetBundleReference GetResult()
        //{
        //    return null;
        //}

        protected bool isFinishedState()
        {
            return m_State == State.Completed || m_State == State.Error;
        }

        public override bool haveError
        {
            get
            {
                return m_State == State.Error;
            }
        }

        public void AddParamTag(string tag)
        {
            m_ParamTags.Add(tag);
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
