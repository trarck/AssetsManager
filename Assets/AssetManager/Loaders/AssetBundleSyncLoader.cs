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
                    if (info.assetBundleInfo.dependencies.Length > 0)
                    {
                        dependencyLoadedSucess=LoadDependencies();
                    }

                    if (dependencyLoadedSucess)
                    {
                        LoadBundle();
                    }
                    else
                    {
                        AMDebug.LogError("[AssetManage]Load AssetBundle with no info");
                        Error();
                    }
                }
                else
                {
					AMDebug.LogError("[AssetManage]Load AssetBundle with no info");
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
            ulong[] dependencies = info.assetBundleInfo.dependencies;
            ResetDependencies();

            for (int i = 0, l = dependencies.Length; i < l; ++i)
            {
                ulong dep = dependencies[i];
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
            if (loaderManager != null && loaderManager.requestManager != null)
            {
                Request request = loaderManager.requestManager.CreateAssetBundleRequest(info);
                request.Start();
                assetBundle = request.assetBundle;
                if (assetBundle != null)
                {
                    //create result
                    m_Result = new AssetBundleReference(assetBundle, info != null ? info.bundleId : 0);
                    m_Result.Retain();
                    if (m_Dependencies != null && m_Dependencies.Count > 0)
                    {
                        m_Result.AddDependencies(m_Dependencies);
                    }
                    m_Result.AddTags(paramTags);

                    Complete();
                }
                else
                {
                    AMDebug.LogFormat("[AssetManage]SyncLoadBundle fail {0}", info.bundleId);
                    Error();
                }
            }
            else
            {
                AMDebug.LogError("[AssetManage]SyncLoadBundle: no request manager.");
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
                return m_Result;
            }
            set
            {
                base.result = value;
            }
        }
    }
}