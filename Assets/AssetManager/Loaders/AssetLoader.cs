﻿using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManage
{
    public abstract class AssetLoader : Loader
    {
        protected AssetReference m_Result;
        //是否在加载后自动断开和AssetBundle的联系。
        protected bool m_AutoReleaseBundle = true;
        private AssetBundleReference m_AssetBundleReference;

        //拆分onComplete和onBeforeComplete，要保证onBeforeComplete和onComplete的执行顺序。
        public event Action<AssetReference> onComplete;

        public event Action<AssetLoader> onBeforeComplete;

        public event Action<AssetLoader> onAfterComplete;

        public AssetLoadInfo info { get; set; }

        public Type type { get; set; }
        public bool autoReleaseBundle
        {
            get
            {
                return m_AutoReleaseBundle;
            }
            set
            {
                if (m_AutoReleaseBundle && value == false)
                {
                    m_AutoReleaseBundle = value;
                }
            }
        }

        public AssetBundleReference assetBundleReference
        {
            get { return m_AssetBundleReference; }
            set
            {

                if (value != null)
                {
                    value.Retain();
                }

                if (m_AssetBundleReference != null)
                {
                    m_AssetBundleReference.Release();
                }

                m_AssetBundleReference = value;
            }
        }

        protected bool AssetIsScene()
        {
            if (info != null)
            {
                return Path.GetExtension(info.path).Equals(".unity", System.StringComparison.CurrentCultureIgnoreCase);
            }
            return false;
        }

		public virtual void Init(Action<AssetLoader> beforeCompleteHandle, Action<AssetLoader> afterCompleteHandle)
		{
			if (state == State.Idle)
			{
				onBeforeComplete += beforeCompleteHandle;
				onAfterComplete += afterCompleteHandle;
				state = Loader.State.Inited;
			}
		}

		protected void DoLoadComplete()
        {
			AMDebug.LogFormat("[AssetManage]AssetLoader {0} DoLoadComplete", info!=null?info.path:"");
            //先调用onBeforeComplete再调用onComplete,否则可能收不到Reference的onDispose事件。
            if (onBeforeComplete != null)
            {
                AMDebug.LogFormat("[AssetManage]AssetLoader {0} onBeforeComplete", info != null ? info.path : "");
				onBeforeComplete(this);
            }

            if (onComplete != null)
            {
                AMDebug.LogFormat("[AssetManage]AssetLoader {0} onComplete", info != null ? info.path : "");
				onComplete(result);
            }

            if (onAfterComplete != null)
            {
                AMDebug.LogFormat("[AssetManage]AssetLoader {0} onAfterComplete", info != null ? info.path : "");
				onAfterComplete(this);
            }
        }

        public override void Clean()
        {
            if (info != null)
            {
                LoadInfoPool.ReleaseAssetLoadInfo(info);
                info = null;
            }

            type = null;
            assetBundleReference = null;
            m_AutoReleaseBundle = false;

            onComplete = null;
            onBeforeComplete = null;
            onAfterComplete = null;

            result = null;
            base.Clean();
        }

		public override void Complete()
		{
			base.Complete();
			DoLoadComplete();
		}

		public override void Error()
		{
			base.Error();

			if (info != null)
			{
				AMDebug.LogErrorFormat("[AssetManage]Load asset {0} fail", info.path);
			}
			DoLoadComplete();
		}

		public virtual AssetReference result
        {
            get
            {
                return m_Result;
            }
            set
            {
                //SetResult(value);
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
    }
}
