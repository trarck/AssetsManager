using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public abstract class AssetBundleLoader : Loader
    {
        protected AssetBundleReference m_Result;

        //这里可以不使用对象池。因为loader已经使用了对象池。
        protected HashSet<AssetBundleReference> m_Dependencies = null;// HashSetPool<AssetBundleReference>.Get();
        //拆分onComplete和onBeforeComplete，要保证onBeforeComplete和onComplete的执行顺序。
        public event Action<AssetBundleReference> onComplete;

        public event Action<AssetBundleLoader> onBeforeComplete;

        public event Action<AssetBundleLoader> onAfterComplete;

        public AssetBundleInfo info { get; set; }

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

            ClearDependencies();

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

        protected void AddDependency(AssetBundleReference abr)
        {
            if (abr != null)
            {
                abr.Retain();
                m_Dependencies.Add(abr);
            }
        }

        protected void ClearDependencies()
        {
            if (m_Dependencies != null)
            {
                foreach (var dep in m_Dependencies)
                {
                    dep.Release();
                }
                m_Dependencies.Clear();
            }
        }

        protected void ResetDependencies()
        {
            if (m_Dependencies == null)
            {
                m_Dependencies = new HashSet<AssetBundleReference>();
            }
            else
            {
                ClearDependencies();
            }
        }
    }
}