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
            Loading,
            Completed,
            Error
        }

        protected State m_State = State.Idle;
        protected bool m_ForceDone = false;


        public int paramLevel { get; set; }
        public string paramTag { get; set; }

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
            paramLevel = 0;
            paramTag = null;
        }
        //public virtual AssetBundleReference GetResult()
        //{
        //    return null;
        //}
    }
}
