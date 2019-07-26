using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetSyncLoader : AssetLoader
    {
        UnityEngine.Object asset { get; set; }

        public override bool isDone
        {
            get
            {
                return true;
            }
        }

        public override void Start()
        {
            if (m_State == State.Inited)
            {
                state = State.Loading;

                if (!string.IsNullOrEmpty(info.bundleName))
                {
                    assetBundleReference=assetManager.LoadAssetBundleSync(info.bundleName, null,false);
                }

                LoadAsset();
            }
            else if (isFinishedState)
            {
                DoLoadComplete();
            }
        }

        void LoadAsset()
        {
            //正常加载Scene，不使用LoadAsset，而使用LoadAssetBundle。
            //这里加入判断防止用错。
            if (!AssetIsScene())
            {
                if (assetBundleReference != null)
                {
                    LoadFromAssetBundle();
                }
                else
                {
                    LoadFromResources();
                }
            }
        }

        void LoadFromAssetBundle()
        {
            if (info != null)
            {
#if SUPPORT_ASSET_ALIAS
                string assetName = info.aliasName;
#else
                string assetName = AssetPaths.AddAssetPrev(info.fullName);
#endif
                if (type == null)
                {
                    asset = assetBundleReference.assetBundle.LoadAsset(assetName);
                }
                else
                {
                    asset = assetBundleReference.assetBundle.LoadAsset(assetName, type);
                }
                Complete();
            }
            else
            {
                Error();
                Debug.LogError("Load Asset with no info");
            }
        }

        void LoadFromResources()
        {
            if (info != null)
            {
                string resourcePath = Path.Combine(Path.GetDirectoryName(info.fullName), Path.GetFileNameWithoutExtension(info.fullName));
                resourcePath = AssetPaths.RemoveAssetPrev(resourcePath);
                if (type == null)
                {
                    asset = Resources.Load(resourcePath);
                }
                else
                {
                    asset = Resources.Load(resourcePath, type);
                }
                Complete();
            }
            else
            {
                Error();
                Debug.LogError("Load Asset with no info");
            }
        }

        public override void Error()
        {
            state = State.Error;
            if (info != null)
            {
                Debug.LogErrorFormat("Load asset {0} fail", info.fullName);
            }
        }

        public override void Clean()
        {
            asset = null;
            base.Clean();
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    //释放托管状态(托管对象)。
                    asset = null;
                }

                // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // 将大型字段设置为 null。
            }
            base.Dispose(disposing);
        }

        public override AssetReference result
        {
            get
            {
                if (state == State.Error)
                {
                    return null;
                }

                if (m_Result == null && state == State.Completed)
                {

                    m_Result = new AssetReference(asset, info.fullName);
                    m_Result.AddTags(paramTags);
                    if (assetBundleReference != null)
                    {
                        m_Result.assetBundleReference = assetBundleReference;
                    }
                }
                return m_Result;
            }
            set
            {
                m_Result = value;
            }
        }
    }
}
