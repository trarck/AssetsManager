using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetBundleSyncLoader : AssetBundleLoader
    {
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
            else if (isFinishedState())
            {
                DoLoadComplete();
            }
        }

        protected override void LoadDependencies()
        {
            string[] dependencies = info.dependencies;

            for (int i = 0, l = dependencies.Length; i < l; ++i)
            {
                string dep = dependencies[i];
                AssetBundleReference abr = assetManager.LoadAssetBundleSync(dep, null,false);
                if (abr != null)
                {
                    m_Dependencies.Add(abr);
                }
            }
        }

        protected override void LoadBundle()
        {
            string assetPath = AssetPaths.GetFullPath(info.fullName);
            Debug.Log("LoadBundle " + assetPath + "," + Time.frameCount);
            LoadFromFileSync(assetPath);
        }

        protected void LoadFromFileSync(string path)
        {
            assetBundle = AssetBundle.LoadFromFile(path);
        }

        public override void Complete()
        {
            state = State.Completed;
        }

        public override void Error()
        {           
            state = State.Error;
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

        public override void Clean()
        {
            m_Result = null;
            HashSetPool<AssetBundleReference>.Release(m_Dependencies);
            m_Dependencies = null;
            base.Clean();
        }
    }
}