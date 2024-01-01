#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YH.AssetManage
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
                AMDebug.Log("[AssetManage]EditorLoader load from resource");
                state = State.Loading;
                LoadFromResources();
            }
            else if (isFinishedState)
            {
                AMDebug.Log("[AssetManage]EditorLoader director complete");
                DoLoadComplete();
            }
        }

        void LoadFromResources()
        {
            if (info != null)
            {
                string resPath = info.path;
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
                AMDebug.LogErrorFormat("[AssetManage]Load asset {0} fail", info.path);
            }
            DoLoadComplete();
        }

        public override void Clean()
        {
            m_Request = null;
            base.Clean();
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
                        m_Result = new AssetReference(m_Request.data, info.pathHash);
                        m_Result.Retain();
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
                //m_Result = value;
                base.result = value;
            }
        }
    }
}
#endif