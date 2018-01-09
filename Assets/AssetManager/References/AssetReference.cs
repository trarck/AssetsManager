using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetReference:BaseReference
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

        public override void Dispose()
        {
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
            base.Reset();
            asset = null;
            assetBundleReference = null;
            name = null;
        }      
    }


    public class AssetRefercenceMonitor : MonoBehaviour
    {
        AssetReference m_AssetReference;

        public AssetReference assetReference
        {
            get
            {
                return m_AssetReference;
            }

            set
            {
                if (m_AssetReference != null)
                {
                    m_AssetReference.Release(gameObject);
                }
                m_AssetReference = value;
                if (m_AssetReference != null)
                {
                    m_AssetReference.Retain(gameObject);
                }            
            }
        }

        void OnDestroy()
        {
            if (m_AssetReference != null)
            {
                m_AssetReference.Release(gameObject);
            }
        }
    }
}
