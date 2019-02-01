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
                Debug.Log("EditorLoader load from resource" + "," + Time.frameCount);
                state = State.Loading;
                LoadFromResources();
            }
            else if (isFinishedState)
            { 
                Debug.Log("EditorLoader director complete" + "," + Time.frameCount);
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
                    m_Request.data= AssetDatabase.LoadMainAssetAtPath(resPath);
                }
                else
                {
                    m_Request = new SyncLoaderRequest();
                    m_Request.data = AssetDatabase.LoadAssetAtPath(resPath, type);
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
            DoLoadComplete();
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
                    if (isDone)
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