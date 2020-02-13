using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    public class LoaderEnumerator : IEnumerator,IDisposable
    {
        public object Current
        {
            get
            {
                return null;
            }
        }

        public virtual bool MoveNext()
        {
            return !isDone;
        }

        public virtual bool isDone
        {
            get
            {
                return true;
            }
        }

        public virtual void Reset()
        {

        }

        public virtual void Dispose()
        {

        }
    }

    public class AssetLoaderEnumerator: LoaderEnumerator
    {
        AssetReference m_AssetReference;
        bool m_LoadComplete = false;

        public override bool isDone
        {
            get
            {
                return m_LoadComplete;
            }
        }

        public void OnAssetLoadComlete(AssetReference assetReference)
        {
            m_LoadComplete = true;
            this.assetReference = assetReference;
        }

        public override void Dispose()
        {
            assetReference = null;
            base.Dispose();
        }

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
                    value.Retain();
                }

                if (m_AssetReference != null)
                {
                    m_AssetReference.Release();
                }

                m_AssetReference = value;
            }
        }
    }

    public class BundleLoaderEnumerator : LoaderEnumerator
    {
        AssetBundleReference m_AssetBundleReference;
        bool m_LoadComplete = false;

        public void OnAssetBundleLoadComlete(AssetBundleReference assetBundleReference)
        {
            m_LoadComplete = true;
            this.assetBundleReference = assetBundleReference;
        }

        public override void Dispose()
        {
            assetBundleReference = null;
            base.Dispose();
        }

        public AssetBundleReference assetBundleReference
        {
            get
            {
                return m_AssetBundleReference;
            }
            set
            {
                if (value != null)
                {
                    value.Retain();
                }

                if (m_AssetBundleReference != null)
                {
                    m_AssetBundleReference.Release();
                }

                m_AssetBundleReference = value;
            }
        }
    }
}
