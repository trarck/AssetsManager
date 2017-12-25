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
            if (m_State == State.Idle)
            {
                state = State.Loading;
                LoadFromResources();
            }
            else if (m_State != State.Loading)
            {
                DoLoadComplete();
            }
        }

        void LoadFromResources()
        {
            if (info != null)
            {
                if (type == null)
                {
                    m_LoaderRequest = new SyncLoaderRequest();
                    m_LoaderRequest.data= AssetDatabase.LoadMainAssetAtPath(info.name);
                }
                else
                {
                    m_LoaderRequest = new SyncLoaderRequest();
                    m_LoaderRequest.data = AssetDatabase.LoadAssetAtPath(info.name, type);
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