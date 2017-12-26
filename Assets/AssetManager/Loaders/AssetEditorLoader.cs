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
        public override void Start()
        {
            if (m_State == State.Inited)
            {
                Debug.Log("EditorLoader load from resource" + "," + Time.frameCount);
                state = State.Loading;
                LoadFromResources();
            }
            else if (isFinishedState())
            { 
                Debug.Log("EditorLoader director complete" + "," + Time.frameCount);
                DoLoadComplete();
            }
        }

        void LoadFromResources()
        {
            if (info != null)
            {
                string resPath = AssetPaths.AddAssetPrev(info.name);
                if (type == null)
                {
                    m_LoaderRequest = new SyncLoaderRequest();
                    m_LoaderRequest.data= AssetDatabase.LoadMainAssetAtPath(resPath);
                }
                else
                {
                    m_LoaderRequest = new SyncLoaderRequest();
                    m_LoaderRequest.data = AssetDatabase.LoadAssetAtPath(resPath, type);
                }                
            }
            else
            {
                Error();
            }
        }       
    }
}
#endif