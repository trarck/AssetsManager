using System;
using System.IO;
using UnityEngine;

namespace YH.AssetManage
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

                if (info.bunldeId>0)
                {
					assetBundleReference = loaderManager.LoadAssetBundleSync(info.bunldeId, 0, AMSetting.CacheDependencyBundle);
                    if (assetBundleReference!=null)
                    {
                        assetBundleReference.Release();
                    }
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
                string assetName = info.path;
                if (string.IsNullOrEmpty(assetName))
                {
                    //TODO use FixeStringPool
                    assetName = HexConverter.ToString(info.pathHash, HexConverter.Casing.Lower);
                }

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
				AMDebug.LogError("[AssetManage]Load Asset with no info");
				Error();
            }
        }

        void LoadFromResources()
        {
            if (info != null)
            {
                string resourcePath = Path.Combine(Path.GetDirectoryName(info.path), Path.GetFileNameWithoutExtension(info.path));
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
				AMDebug.LogError("[AssetManage]Load Asset with no info");
                Error();
            }
        }

        public override void Error()
        {
            state = State.Error;
            if (info != null)
            {
				AMDebug.LogErrorFormat("[AssetManage]Load asset {0} fail", info.path);
            }
        }

        public override void Clean()
        {
            asset = null;
            base.Clean();
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

                    m_Result = new AssetReference(asset, info.pathHash);
                    m_Result.Retain();
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
                //SetResult(value);
                base.result = value;
            }
        }
    }
}
