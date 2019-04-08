using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManager
{
    public abstract class AssetLoader : Loader
    {
        protected AssetReference m_Result;
        //拆分onComplete和onBeforeComplete，要保证onBeforeComplete和onComplete的执行顺序。
        public Action<AssetReference> onComplete;

        public Action<AssetLoader> onBeforeComplete;

        public Action<AssetLoader> onAfterComplete;

        public AssetInfo info { get; set; }

        public Type type { get; set; }

        public AssetBundleReference assetBundleReference { get; set; }

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
            Debug.Log("EditorLoader DoLoadComplete" + "," + Time.frameCount);
#endif
            //先调用onBeforeComplete再调用onComplete,否则可能收不到Reference的onDispose事件。
            if (onBeforeComplete != null)
            {
#if ASSETMANAGER_LOG
                Debug.Log("EditorLoader onBeforeComplete" + "," + Time.frameCount);
#endif
                onBeforeComplete(this);
            }

            if (onComplete != null)
            {
#if ASSETMANAGER_LOG
                Debug.Log("EditorLoader onComplete" + "," + Time.frameCount);
#endif
                onComplete(result);
            }

            if (onAfterComplete != null)
            {
#if ASSETMANAGER_LOG
                Debug.Log("EditorLoader onAfterComplete" + "," + Time.frameCount);
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
