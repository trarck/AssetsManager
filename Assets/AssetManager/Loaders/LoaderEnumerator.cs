using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
{
    public class LoaderEnumerator : IEnumerator,IDisposable
    {
        protected bool m_LoadComplete = false;
		private bool m_DisposedValue = false;

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
                return m_LoadComplete;
            }
        }

        public virtual void Reset()
        {

        }

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// TODO: 释放托管状态(托管对象)。
			}

			// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
			// TODO: 将大型字段设置为 null。
		}

		// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
		~LoaderEnumerator()
		{
			// 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
			if (!m_DisposedValue)
			{
				Dispose(false);
				m_DisposedValue = true;
			}
		}

		// 添加此代码以正确实现可处置模式。
		public void Dispose()
		{
			// 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
			if (!m_DisposedValue)
			{
				Dispose(true);
				m_DisposedValue = true;
			}
			// TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
			GC.SuppressFinalize(this);
		}
	}

    public class AssetLoaderEnumerator: LoaderEnumerator
    {
        AssetReference m_AssetReference;

        public void OnAssetLoadComlete(AssetReference assetReference)
        {
            m_LoadComplete = true;
            this.assetReference = assetReference;
        }

		protected override void Dispose(bool disposing)
        {
			assetReference = null;
            base.Dispose(disposing);
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

        public void OnAssetBundleLoadComlete(AssetBundleReference assetBundleReference)
        {
            m_LoadComplete = true;
            this.assetBundleReference = assetBundleReference;
        }

        protected override void Dispose(bool disposing)
        {
            assetBundleReference = null;
            base.Dispose(disposing);
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
