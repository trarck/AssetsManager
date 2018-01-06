using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetBundleLoader : Loader
    {
        LoaderRequest m_LoaderRequest;

        AssetBundleReference m_Result;

        int m_activeDependencyLoader = 0;

        HashSet<AssetBundleReference> m_Dependencies = HashSetPool<AssetBundleReference>.Get();

        public Action<AssetBundleReference> onComplete;

        public Action<AssetBundleLoader> onLoaded;

        public override bool isDone
        {
            get
            {
                return forceDone || m_LoaderRequest != null && m_LoaderRequest.isDone;
            }
        }

        public AssetBundleInfo info { get; set; }

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
                    Debug.LogError("AssetBundleLoader can't start without info");
                }
            }
            else if (isFinishedState())
            {
                DoLoadComplete();
            }
        }

        protected void LoadBundle()
        {
            string assetPath = AssetPaths.GetFullPath(info.fullName);
            Debug.Log("LoadBundle " + assetPath + "," + Time.frameCount);
            if (assetPath.Contains("://"))
            {
                this.LoadFromPackage(assetPath);
            }
            else
            {
                this.LoadFromFile(assetPath);
            }
        }

        protected void LoadDependencies()
        {
            string[] dependencies = info.dependencies;
            m_activeDependencyLoader = dependencies.Length;

            Debug.Log("Load Dependencies " + m_activeDependencyLoader+","+Time.frameCount);

            for (int i = 0, l = dependencies.Length;i< l;++i)
            {
                string dep = dependencies[i];
                assetManager.LoadAssetBundle(dep, OnDependencyComplete);
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
            if (m_LoaderRequest != null && m_LoaderRequest.assetBundle != null)
            {
                state = State.Completed;
                DoLoadComplete();
            }
            else
            {
                Error();
            }
        }

        public override void Error()
        {
            state = State.Error;
            DoLoadComplete();
        }

        protected void DoLoadComplete()
        {
            //选调用onLoaded再调用onComplete,否则可能收不到Reference的onDispose事件。
            if (onLoaded != null)
            {
                onLoaded(this);
            }

            if (onComplete != null)
            {
                onComplete(this.result);
            }
        }

        protected LoaderRequest LoadFromFile(string path)
        {
            m_LoaderRequest =new BundleLoaderRequest(AssetBundle.LoadFromFileAsync(path));
            return m_LoaderRequest;
        }

        protected LoaderRequest LoadFromPackage(string path)
        {
            m_LoaderRequest = new WWWRequest(new WWW(path));
            return m_LoaderRequest;
        }

        public AssetBundleReference result
        {
            get
            {
                if (m_Result == null)
                {
                    if (isDone)
                    {
                        m_Result = new AssetBundleReference(m_LoaderRequest.assetBundle, info.fullName);

                        m_Result.AddTags(paramTags);

                        if (m_Dependencies!=null && m_Dependencies.Count > 0)
                        {
                            m_Result.AddDependencies(m_Dependencies);
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

        public override void Clean()
        {
            onComplete = null;
            m_LoaderRequest = null;
            m_Result = null;
            HashSetPool<AssetBundleReference>.Release(m_Dependencies);
            m_Dependencies = null;
            base.Clean();
        }
    }
}