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

        public Action<AssetBundleReference> onComplete;

        public Action<AssetBundleLoader> onLoaded;

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
            //选调用onLoaded再调用onComplete,否则可能收不到Reference的onDispose事件。
            if (onLoaded != null)
            {
                onLoaded(this);
            }

            if (onComplete != null)
            {
                onComplete(result);
            }
        }

        public override void Clean()
        {
            onComplete = null;
            m_Result = null;
            HashSetPool<AssetBundleReference>.Release(m_Dependencies);
            m_Dependencies = null;
            base.Clean();
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