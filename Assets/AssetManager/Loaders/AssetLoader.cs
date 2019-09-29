using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManager
{
    public abstract class AssetLoader : Loader
    {
        protected AssetReference m_Result;
        //拆分onComplete和onBeforeComplete，要保证onBeforeComplete和onComplete的执行顺序。
        public event Action<AssetReference> onComplete;

        public event Action<AssetLoader> onBeforeComplete;

        public event Action<AssetLoader> onAfterComplete;

        public AssetInfo info { get; set; }

        public Type type { get; set; }

        private AssetBundleReference m_AssetBundleReference;
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
                return Path.GetExtension(info.fullName).Equals(".unity", System.StringComparison.CurrentCultureIgnoreCase);
            }
            return false;
        }

        protected void DoLoadComplete()
        {
#if ASSETMANAGER_LOG
            Debug.LogFormat("AssetLoader {0}, DoLoadComplete,{1}", info!=null?info.fullName:"", Time.frameCount);
#endif
            //先调用onBeforeComplete再调用onComplete,否则可能收不到Reference的onDispose事件。
            if (onBeforeComplete != null)
            {
#if ASSETMANAGER_LOG
                Debug.LogFormat("AssetLoader {0}, onBeforeComplete,{1}", info != null ? info.fullName : "", Time.frameCount);
#endif
                onBeforeComplete(this);
            }

            if (onComplete != null)
            {
#if ASSETMANAGER_LOG
                Debug.LogFormat("AssetLoader {0}, onComplete,{1}", info != null ? info.fullName : "", Time.frameCount);
#endif
                onComplete(result);
            }

            if (onAfterComplete != null)
            {
#if ASSETMANAGER_LOG
                Debug.LogFormat("AssetLoader {0}, onAfterComplete,{1}", info != null ? info.fullName : "", Time.frameCount);

#endif
                onAfterComplete(this);
            }
        }

        public override void Clean()
        {
            onComplete = null;
            onBeforeComplete = null;
            onAfterComplete = null;
            info = null;
            type = null;
            assetBundleReference = null;

            m_Result = null;
            base.Clean();
        }

        public virtual AssetReference result
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
