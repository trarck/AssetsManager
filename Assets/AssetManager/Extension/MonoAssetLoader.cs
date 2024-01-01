using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage.Extension
{
    public class MonoAssetLoader:MonoBehaviour
    {
        bool m_Destroyed = false;

        //protected Dictionary<string, Action<AssetReference>> m_Handles=new Dictionary<string, Action<AssetReference>>();

        private void OnDestroy()
        {
            m_Destroyed = true;
        }

        public void LoadAsset(string path, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            LoadAsset(path, 0, null, completeHandle, autoReleaseBundle);
        }

        public void LoadAsset<T>(string path, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            LoadAsset(path, 0, typeof(T), completeHandle, autoReleaseBundle);
        }

        public void LoadAsset<T>(string path, string tag, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            LoadAsset(path, 0, typeof(T), completeHandle, autoReleaseBundle);
        }

        public void LoadAsset(string path, int tag, Type type, Action<AssetReference> completeHandle = null, bool autoReleaseBundle = true)
        {
            if (!m_Destroyed)
            {
                //这里暂时使用匿名函数。
                AssetManager.Instance.LoadAsset(path, tag, type, autoReleaseBundle, (assetReference) =>
                {
                //如果已经删除，则不处理。
                if (!m_Destroyed)
                    {
                        if (completeHandle != null)
                        {
                            completeHandle(assetReference);
                        }
                    }
                });
            }
        }

        //protected void OnAssetLoadComplete(AssetReference assetReference)
        //{
        //    if (assetReference != null)
        //    {
        //    }
        //}

        public void LoadAssetBundle(ulong bundleId, bool cacheLoadedAsset, Action<AssetBundleReference> completeHandle = null)
        {
            LoadAssetBundle(bundleId, 0, cacheLoadedAsset, completeHandle);
        }

        public void LoadAssetBundle(ulong path, int tag, bool cacheLoadedAsset, Action<AssetBundleReference> completeHandle = null)
        {
            if (!m_Destroyed)
            {
                //这里暂时使用匿名函数。
                AssetManager.Instance.LoadAssetBundle(path, tag, cacheLoadedAsset, (assetBundleReference) =>
                {
                    //如果已经删除，则不处理。
                    if (!m_Destroyed)
                    {
                        if (completeHandle != null)
                        {
                            completeHandle(assetBundleReference);
                        }
                    }
                });
            }
        }

        public static MonoAssetLoader Create(GameObject target=null)
        {
            MonoAssetLoader loader = null;
            if (target == null)
            {
                target = new GameObject();
                loader=target.AddComponent<MonoAssetLoader>();
            }
            else
            {
                loader = target.GetComponent<MonoAssetLoader>();
                if (loader == null)
                {
                    loader = target.AddComponent<MonoAssetLoader>();
                }
            }
            return loader;
        }
    }
}
