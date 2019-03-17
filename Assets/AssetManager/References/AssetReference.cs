using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetReference : BaseReference
    {
        AssetBundleReference m_AssetBundleReference;

        public Object asset { get; set; }

        public delegate void DisposeHandle(AssetReference abr);

        public event DisposeHandle onDispose;

        public AssetBundleReference assetBundleReference
        {
            get
            {
                return m_AssetBundleReference;
            }
            set
            {

                //retain new
                if (value != null)
                {
                    value.Retain();
                }

                //release old

                if (m_AssetBundleReference != null)
                {
                    m_AssetBundleReference.Release();
                }
                m_AssetBundleReference = value;
            }
        }

        public AssetReference(Object asset, string assetPath)
        {
            this.asset = asset;
            this.name = assetPath;
        }

        /// <summary>
        /// 直接监视资源
        /// </summary>
        /// <param name="gameObject"></param>
        public override void Monitor(GameObject gameObject)
        {
            if (gameObject != null)
            {
                AssetRefercenceMonitor monitor = gameObject.GetComponent<AssetRefercenceMonitor>();
                if (monitor == null)
                {
                    monitor = gameObject.AddComponent<AssetRefercenceMonitor>();
                }
                monitor.assetReference = this;
            }
        }

        /// <summary>
        /// 按tag来监视资源
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        public override void Monitor(GameObject gameObject,int tag)
        {
            if (gameObject != null)
            {
                AssetRefercenceMonitor monitor = gameObject.GetComponent<AssetRefercenceMonitor>();
                if (monitor == null)
                {
                    monitor = gameObject.AddComponent<AssetRefercenceMonitor>();
                }
                monitor.AddAssetReference(this, tag);
            }
        }

        public override void Dispose()
        {
#if ASSETMANAGER_LOG
            Debug.Log("Asset dispose " + name + "," + Time.frameCount);
#endif
            if (onDispose != null)
            {
                onDispose(this);
                onDispose = null;
            }

            if (asset != null)
            {
                if (!(asset is GameObject))
                {
                    Resources.UnloadAsset(asset);
                }

                asset = null;
            }

            asset = null;
            assetBundleReference = null;
            name = null;

            base.Dispose();
        }


        public override void Reset()
        {
            asset = null;
            assetBundleReference = null;
            onDispose = null;
            base.Reset();
        }
    }


    public class AssetRefercenceMonitor : MonoBehaviour
    {
        AssetReference m_AssetReference;

        Dictionary<int, AssetReference> m_TagReferences=null;

        public AssetReference assetReference
        {
            get
            {
                return m_AssetReference;
            }

            set
            {
                if (value != null)
                {
                    value.Retain(gameObject);
                }

                if (m_AssetReference != null)
                {
                    m_AssetReference.Release(gameObject);
                }
                m_AssetReference = value;
            }
        }

        /// <summary>
        /// 按tag来保存资源引用
        /// </summary>
        /// <param name="assetRef"></param>
        /// <param name="tag"></param>
        public void AddAssetReference(AssetReference assetRef,int tag)
        {
            //检查参数
            if (assetRef == null)
            {
                return;
            }

            //检查字典
            if (m_TagReferences == null)
            {
                m_TagReferences = new Dictionary<int, AssetReference>();
            }

            //先增加引用
            assetRef.Retain();

            //再释放旧引用
            AssetReference oldRef;
            if (m_TagReferences.TryGetValue(tag, out oldRef))
            {
                if (oldRef != null)
                {
                    oldRef.Release();
                }
            }
            //保存
            m_TagReferences[tag] = assetRef;
        }

        /// <summary>
        /// 移除某个tag的资源引用
        /// </summary>
        /// <param name="tag"></param>
        public void RemoveAssetReference(int tag)
        {

            if (m_TagReferences == null)
            {
                return;
            }
            //释放引用
            AssetReference assetRef;
            if (m_TagReferences.TryGetValue(tag, out assetRef))
            {
                m_TagReferences.Remove(tag);
                if (assetRef != null)
                {
                    assetRef.Release();
                }
            }
        }

        /// <summary>
        /// 从资源tag字典中移除引用。
        /// </summary>
        /// <param name="assetRef"></param>
        public void RemoveAssetReference(AssetReference assetRef)
        {

            if (m_TagReferences == null)
            {
                return;
            }

            foreach (var iter in m_TagReferences)
            {
                if(iter.Value== assetRef)
                {
                    m_TagReferences.Remove(iter.Key);
                    assetRef.Release();
                    break;
                }
            }
        }

        void OnDestroy()
        {
            if (m_AssetReference != null)
            {
                m_AssetReference.Release(gameObject);
            }

            if (m_TagReferences != null)
            {
                foreach(var iter in m_TagReferences)
                {
                    iter.Value.Release();
                }
                m_TagReferences = null;
            }
        }
    }
}
