using System;
using System.Collections;
using System.Collections.Generic;

namespace YH.AssetManager
{
    public abstract class Loader : IEnumerator
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

        HashSet<string> m_ParamTags = HashSetPool<string>.Get();

        public abstract bool isDone { get; }

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
            HashSetPool<string>.Release(m_ParamTags);
            m_ParamTags = null;
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

        public bool MoveNext()
        {
            return !isDone || !isFinishedState;
        }

        public void Reset()
        {
            UnityEngine.Debug.Log("####3Reset");
        }

        public object Current
        {
            get
            {
                return null;
            }
        }
    }
}
