using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetBundleSyncLoader : AssetBundleLoader
    {
        AssetBundle assetBundle { get; set; }

        public override bool isDone
        {
            get
            {
                return true;
            }
        }

        public override void Start()
        {
            if (m_State == State.Inited)
            {
                if (info != null)
                {
                    state = State.Loading;

                    if (info.dependencies.Length > 0)
                    {
                        LoadDependencies();
                    }

                    LoadBundle();
                }
                else
                {
                    Error();
                }
            }
            else if (isFinishedState)
            {
                DoLoadComplete();
            }
        }

        void LoadDependencies()
        {
            string[] dependencies = info.dependencies;

            for (int i = 0, l = dependencies.Length; i < l; ++i)
            {
                string dep = dependencies[i];
                AssetBundleReference abr = assetManager.LoadAssetBundleSync(dep, null, false);
                if (abr != null)
                {
                    m_Dependencies.Add(abr);
                }
            }
        }

        void LoadBundle()
        {
            string assetPath = AssetPaths.GetFullPath(info.fullName);
#if ASSETMANAGER_LOG
            Debug.Log("LoadBundle " + assetPath + "," + Time.frameCount);
#endif
            LoadFromFileSync(assetPath);
            Complete();
        }

        protected void LoadFromFileSync(string path)
        {
            assetBundle = AssetBundle.LoadFromFile(path);
        }

        public override void Clean()
        {
            assetBundle = null;
            base.Clean();
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    //释放托管状态(托管对象)。
                    assetBundle = null;
                }

                // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // 将大型字段设置为 null。
            }
            base.Dispose(disposing);
        }

        public override AssetBundleReference result
        {
            get
            {
                if (state == State.Error)
                {
                    return null;
                }

                if (m_Result == null && state == State.Completed)
                {
                    m_Result = new AssetBundleReference(assetBundle, info != null ? info.fullName : "");
                    if (m_Dependencies != null && m_Dependencies.Count > 0)
                    {
                        m_Result.AddDependencies(m_Dependencies);
                    }
                    m_Result.AddTags(paramTags);
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