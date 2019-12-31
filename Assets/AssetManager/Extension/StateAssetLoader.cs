using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage.Extension
{
    public abstract class StateAssetLoader 
    {
        protected enum State
        {
            Idle,
            Loading,
            Loaded
        }

        protected State m_State;

        public event Action<AssetReference> onComplete;

        public void LoadAsset(string path,  bool autoReleaseBundle = true)
        {
            LoadAsset(path, 0, null, autoReleaseBundle);
        }

        public void LoadAsset<T>(string path, bool autoReleaseBundle = true)
        {
            LoadAsset(path, 0, typeof(T), autoReleaseBundle);
        }

        public void LoadAsset<T>(string path, string tag, bool autoReleaseBundle = true)
        {
            LoadAsset(path, 0, typeof(T), autoReleaseBundle);
        }

        public virtual void LoadAsset(string path, int tag, Type type, bool autoReleaseBundle = true)
        {

        }

        protected void TriggerCompleteEvent(AssetReference assetReference)
        {
            if (onComplete != null)
            {
                onComplete(assetReference);
            }
        }
    }
}
