#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YH.AssetManager
{
    public class AssetEditorLoader : AssetLoader
    {
        protected Request m_Request;

        public override bool isDone
        {
            get
            {
                return forceDone || m_Request != null && m_Request.isDone;
            }
        }

        public override void Start()
        {
            if (m_State == State.Inited)
            {
#if ASSETMANAGER_LOG
                Debug.Log("EditorLoader load from resource" + "," + Time.frameCount);
#endif
                state = State.Loading;
                LoadFromResources();
            }
            else if (isFinishedState)
            {
#if ASSETMANAGER_LOG
                Debug.Log("EditorLoader director complete" + "," + Time.frameCount);
#endif
                DoLoadComplete();
            }
        }

        void LoadFromResources()
        {
            if (info != null)
            {
                string resPath = AssetPaths.AddAssetPrev(info.fullName);
                if (type == null)
                {
                    m_Request = new SyncLoaderRequest();
                    m_Request.data = AssetDatabase.LoadMainAssetAtPath(resPath);
                }
                else
                {
                    m_Request = new SyncLoaderRequest();
                    m_Request.data = AssetDatabase.LoadAssetAtPath(resPath, type);
                }
                if (m_Request.data != null)
                {
                    Complete();
                }
                else
                {
                    Error();
                }
            }
            else
            {
                Error();
            }
        }

        public override void Complete()
        {
            base.Complete();
            DoLoadComplete();
        }

        public override void Error()
        {
            base.Error();
            if (info != null)
            {
                Debug.LogErrorFormat("Load asset {0} fail", info.fullName);
            }
            DoLoadComplete();
        }

        public override void Clean()
        {
            m_Request = null;
            base.Clean();
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    //释放托管状态(托管对象)。
                    m_Request = null;
                }

                // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // 将大型字段设置为 null。
            }
            base.Dispose(disposing);
        }


        public override AssetReference result
        {
            get
            {
                if (state == State.Error)
                {
                    return null;
                }

                if (m_Result == null && state == State.Completed)
                {
                    if (isDone && m_Request.data!=null)
                    {
                        m_Result = new AssetReference(m_Request.data, info.fullName);
                        m_Result.AddTags(paramTags);
                        if (assetBundleReference != null)
                        {
                            m_Result.assetBundleReference = assetBundleReference;
                        }
                    }
                }
                return m_Result;
            }
            set
            {
                m_Result = value;
            }
        }
    }
}
#endif