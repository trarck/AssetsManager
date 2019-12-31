using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage.Extension
{
    public class MutexAssetLoader
    {
        public enum State
        {
            Idle,
            Loading,
            Loaded
        }

        State m_State;

        bool m_Cancel = false;

        AssetLoader m_InternalLoader;

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

        public void LoadAsset(string path, int tag, Type type, bool autoReleaseBundle = true)
        {
            if (m_State == State.Loading)
            {
#if ASSETMANAGER_LOG
                Debug.LogFormat("LoadAsset State is loading.Remove last load new {0}", path);
#endif
                //取消上个资源的回调
                m_InternalLoader.onComplete -= OnAssetLoadComplete;
                //加载新的资源
                m_InternalLoader = AssetManager.Instance.LoadAsset(path, tag, type, OnAssetLoadComplete, autoReleaseBundle);
            }
            else 
            {
#if ASSETMANAGER_LOG
                Debug.LogFormat("LoadAsset {0}", path);
#endif
                m_State = State.Loading;
                m_InternalLoader = AssetManager.Instance.LoadAsset(path, tag, type, OnAssetLoadComplete, autoReleaseBundle);
            }
        }

        protected void OnAssetLoadComplete(AssetReference assetReference)
        {
            m_State = State.Loaded;
            DoComplete(assetReference);
        }

        protected void DoComplete(AssetReference assetReference)
        {
            m_InternalLoader = null;
            if (onComplete != null)
            {
                onComplete(assetReference);
            }
        }
    }
}
