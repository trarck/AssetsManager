using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    /// <summary>
    /// 资源从AssetBundle加载出来后，会重新生成一块内存。此时可以调用AssetBundle的unload(false)，释放AssetBundle占用的内存。
    /// 如果AssetBundle中有多个资源，可以用预加载的方式加载AssetBundle，加载完所有资源后，再把AssetBundle删除。
    ///                              也可以在资源加载完成后，把每个资源对AssetBundle的引用断开。
    /// </summary>
    public class AssetReference : BaseReference
    {
        //保持的AssetBundle的引用，如果要断开引用，只要赋值null。或调用RelaseBundleReference.
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
                    Debug.LogFormat("AssetReference:Release {0},{1}",name, m_AssetBundleReference!=null? m_AssetBundleReference.name:"Null");
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

        /// <summary>
        /// 释放到AssetBundle的引用。如果此时AssetBundle没有被其它资源引用，则会调用Unload(false)
        /// </summary>
        public void ReleaseBundleReference()
        {
            assetBundleReference = null;
        }

        public override void Dispose(bool disposing=false)
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
            //这里通过setter调用release
            assetBundleReference = null;
            name = null;

            base.Dispose(disposing);
        }

        public override void Reset()
        {
            asset = null;
            assetBundleReference = null;
            onDispose = null;
            base.Reset();
        }

        //public override void Retain()
        //{
        //    Debug.LogFormat("{0} Retain", asset);
        //    base.Retain();
        //}
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
                if (m_AssetReference != value)
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
