using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManager
{
    public abstract class AssetLoader : Loader
    {
        protected AssetReference m_Result;

        public Action<AssetReference> onComplete;

        public Action<AssetLoader> onLoaded;

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
