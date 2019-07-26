using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public abstract class Request : DataEnumerator,IDisposable
    {
        public Action<Request> onComplete;
        public bool autoRelease = true;

        #region IDisposable Support
        protected bool m_Disposed = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    //释放托管状态(托管对象)。
                }

                //释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                //将大型字段设置为 null。

                m_Disposed = true;
            }
        }

        //仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        //~Request()
        //{
        //    // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //    Dispose(false);
        //}

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            //如果在以上内容中替代了终结器，则取消注释以下行。
            //GC.SuppressFinalize(this);
        }
        #endregion

        public virtual void Start()
        {

        }

        public virtual void Update()
        {
            #if ASSETMANAGER_LOG
            Debug.LogFormat("{0},{1},{2}", this, isDone,Time.frameCount);
            #endif
        }

        public virtual void Complete()
        {
            if (onComplete!=null)
            {
                onComplete(this);
            }
        }

        public virtual void Clean()
        {
            onComplete=null;
            autoRelease = true;
        }
    }
}
