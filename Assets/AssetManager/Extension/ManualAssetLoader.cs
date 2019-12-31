using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage.Extension
{
    public class ManualAssetLoader : StateAssetLoader
    {
        bool m_Cancel = false;

        public override void LoadAsset(string path, int tag, Type type, bool autoReleaseBundle = true)
        {
            //如果已经加载，则忽略后续加载请求。
            if (m_State == State.Idle)
            {
                m_State = State.Loading;
                AssetManager.Instance.LoadAsset(path, tag, type, OnAssetLoadComplete, autoReleaseBundle);
            }
        }

        protected void OnAssetLoadComplete(AssetReference assetReference)
        {
            m_State = State.Loaded;
            //如果取消加载，则不触发事件。
            if (!m_Cancel)
            {
                TriggerCompleteEvent(assetReference);
            }
        }
    }
}
