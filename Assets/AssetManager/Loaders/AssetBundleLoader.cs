﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
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

        public event Action<AssetBundleLoader> onAssetBundleLoaded;

        public AssetBundleLoadInfo info { get; set; }

		public virtual void Init(Action<AssetBundleLoader> beforeCompleteHandle, Action<AssetBundleLoader> afterCompleteHandle)
		{
			if (state == State.Idle)
			{
				//对加载前后做特殊处理
				onBeforeComplete += beforeCompleteHandle;
				onAfterComplete += afterCompleteHandle;
				state = Loader.State.Inited;
			}
		}

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

        protected void DoAssetBundleLoaded()
        {
            if (onAssetBundleLoaded != null)
            {
                onAssetBundleLoaded(this);
            }
        }

        public override void Clean()
        {
            onComplete = null;
            onBeforeComplete = null;
            onAfterComplete = null;
            onAssetBundleLoaded = null;

            result = null;

            if (info != null)
            {
                LoadInfoPool.ReleaseAssetBundleLoadInfo(info);
                info = null;
            }

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
                if (value != null)
                {
                    value.Retain();
                }

                if (m_Result != null)
                {
                    m_Result.Release();
                }
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

        public override void Complete()
        {
			base.Complete();
            DoLoadComplete();
        }

        public override void Error()
        {
			base.Error();
            DoLoadComplete();
        }
    }
}