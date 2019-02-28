using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public abstract class AssetBundleLoader : Loader
    {
        protected AssetBundleReference m_Result;

        protected HashSet<AssetBundleReference> m_Dependencies = HashSetPool<AssetBundleReference>.Get();
        //拆分onComplete和onBeforeComplete，要保证onBeforeComplete和onComplete的执行顺序。
        public Action<AssetBundleReference> onComplete;

        public Action<AssetBundleLoader> onBeforeComplete;

        public Action<AssetBundleLoader> onAfterComplete;

        public AssetBundleInfo info { get; set; }

        //public override void Complete()
        //{
        //    state = State.Completed;
        //    DoLoadComplete();
        //}

        //public override void Error()
        //{
        //    state = State.Error;
        //    DoLoadComplete();
        //}

        protected void DoLoadComplete()
        {
            //先调用onBeforeComplete再调用onComplete,否则可能收不到Reference的onDispose事件。
            if (onBeforeComplete != null)
            {
                onBeforeComplete(this);
            }

            if (onComplete != null)
            {
                onComplete(result);
            }

            if (onAfterComplete != null)
            {
                onAfterComplete(this);
            }
        }

        public override void Clean()
        {
            onComplete = null;
            onBeforeComplete = null;
            onAfterComplete = null;

            m_Result = null;

            info = null;

            if (m_Dependencies != null)
            {                
                m_Dependencies.Clear();
            }
            base.Clean();
        }

        protected override void Dispose(bool disposing)
        {
            Clean();
            if (m_Dependencies != null)
            {
                HashSetPool<AssetBundleReference>.Release(m_Dependencies);
                m_Dependencies = null;
            }
            base.Dispose(disposing);
        }

        public virtual AssetBundleReference result
        {
            get
            {
                return m_Result;
            }
            set
            {
                m_Result = value;
            }
        }
    }
}