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
                AMDebug.LogFormat("[AssetManage]LoadAsset State is loading.Remove last load new {0}", path);
                //取消上个资源的回调
                m_InternalLoader.onComplete -= OnAssetLoadComplete;
                //加载新的资源
                m_InternalLoader = AssetManager.Instance.LoadAsset(path, tag, type, autoReleaseBundle, OnAssetLoadComplete).loader;
            }
            else 
            {
                AMDebug.LogFormat("[AssetManage]LoadAsset {0}", path);
                m_State = State.Loading;
                m_InternalLoader = AssetManager.Instance.LoadAsset(path, tag, type, autoReleaseBundle, OnAssetLoadComplete).loader;
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
