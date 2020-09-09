using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
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
                    bool dependencyLoadedSucess = true;
                    if (info.dependencies.Length > 0)
                    {
                        dependencyLoadedSucess=LoadDependencies();
                    }

                    if (dependencyLoadedSucess)
                    {
                        LoadBundle();
                    }
                    else
                    {
                        Debug.LogError("[AssetManage]Load AssetBundle with no info");
                        Error();
                    }
                }
                else
                {
                    Debug.LogError("[AssetManage]Load AssetBundle with no info");
                    Error();
                }
            }
            else if (isFinishedState)
            {
                DoLoadComplete();
            }
        }

        bool LoadDependencies()
        {
            string[] dependencies = info.dependencies;
            ResetDependencies();

            for (int i = 0, l = dependencies.Length; i < l; ++i)
            {
                string dep = dependencies[i];
                AssetBundleReference abr = loaderManager.LoadAssetBundleSync(dep, 0, AMSetting.CacheDependencyBundle);
                if (abr == null)
                {
                    if (AMSetting.BreakOnBundleLoadDependencyError)
                    {
                        ClearDependencies();
                        return false;
                    }
                }
                else
                {
                    AddDependency(abr);
                    //must release 
                    abr.Release();
                }
            }
            return true;
        }

        void LoadBundle()
        {
            string assetPath = AssetPaths.GetFullPath(info.fullName);
#if ASSETMANAGER_LOG_ON
            Debug.Log("[AssetManage]LoadBundle " + assetPath + "," + Time.frameCount);
#endif
            LoadFromFileSync(assetPath);
            
        }

        protected void LoadFromFileSync(string path)
        {
            assetBundle = AssetBundle.LoadFromFile(path);
            if (assetBundle != null)
            {
                Complete();
            }
            else
            {
#if ASSETMANAGER_LOG_ON
                Debug.Log("[AssetManage]LoadBundle fail " + path + "," + Time.frameCount);
#endif
                Error();
            }
        }

        public override void Clean()
        {
            assetBundle = null;
            base.Clean();
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
                    m_Result.Retain();
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
                base.result = value;
            }
        }
    }
}