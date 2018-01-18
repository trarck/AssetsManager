using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetBundleAsyncLoader : AssetBundleLoader
    {
        LoaderRequest m_LoaderRequest;

        int m_activeDependencyLoader = 0;

        public override bool isDone
        {
            get
            {
                return forceDone || m_LoaderRequest != null && m_LoaderRequest.isDone;
            }
        }

        public override void Start()
        {
            if (m_State == State.Inited)
            {
                if (info != null)
                {
                    state = State.Loading;

                    if (info.dependencies.Length>0)
                    {
                        LoadDependencies();
                    }
                    else
                    {
                        LoadBundle();
                    }
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

        void LoadBundle()
        {
            string assetPath = AssetPaths.GetFullPath(info.fullName);
            Debug.Log("LoadBundle " + assetPath + "," + Time.frameCount);
            if (assetPath.Contains("://"))
            {
                LoadFromPackage(assetPath);
            }
            else
            {
                LoadFromFile(assetPath);
            }
        }

        void LoadDependencies()
        {
            string[] dependencies = info.dependencies;
            m_activeDependencyLoader = dependencies.Length;

            Debug.Log("Load Dependencies " + m_activeDependencyLoader+","+Time.frameCount);

            for (int i = 0, l = dependencies.Length;i< l;++i)
            {
                string dep = dependencies[i];
                assetManager.LoadAssetBundle(dep, false,OnDependencyComplete);
            }
        }

        protected void OnDependencyComplete(AssetBundleReference abr)
        {
            m_Dependencies.Add(abr);
            if (--m_activeDependencyLoader == 0)
            {
                LoadBundle();
            }
        }

        public override void Complete()
        {
            if (m_LoaderRequest != null && !m_LoaderRequest.haveError)
            {
                state = State.Completed;
                DoLoadComplete();
            }
            else
            {
                Debug.LogError("AssetBundleLoader fail load "+info.fullName);
                Error();
            }
        }

        public override void Error()
        {           
            state = State.Error;
            DoLoadComplete();
        }

        LoaderRequest LoadFromFile(string path)
        {
            m_LoaderRequest =new BundleLoaderRequest(AssetBundle.LoadFromFileAsync(path));
            return m_LoaderRequest;
        }

        LoaderRequest LoadFromPackage(string path)
        {
            m_LoaderRequest = new WWWRequest(new WWW(path));
            return m_LoaderRequest;
        }

        public override void Clean()
        {
            m_LoaderRequest = null;
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
                    if (isDone)
                    {
                        m_Result = new AssetBundleReference(m_LoaderRequest.assetBundle,info!=null ? info.fullName:"");
                        if (m_Dependencies != null && m_Dependencies.Count > 0)
                        {
                            m_Result.AddDependencies(m_Dependencies);
                        }
                        m_Result.AddTags(paramTags);
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