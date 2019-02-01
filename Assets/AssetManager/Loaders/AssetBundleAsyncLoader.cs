﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    /// <summary>
    /// 异步加载AssetBundle
    /// AssetBundle和依赖项同时加载，判断一个AssetBundle是加载完成，要检查自己是否加载和所有依赖是否完成。
    /// </summary>
    public class AssetBundleAsyncLoader : AssetBundleLoader
    {
        public Action<AssetBundleAsyncLoader> onAssetBundleLoaded;

        int m_WaitDependencyLoadCount = 0;
        bool m_DependenciesIsLoaded = false;

        int m_WaitDependencyCompleteCount = 0;
        bool m_DependenciesIsDone = false;

        List<AssetBundleAsyncLoader> m_DependencyLoaders = null;
        /// <summary>
        /// AssetBundle和所有依赖都加载完成。
        /// </summary>
        public override bool isDone
        {
            get
            {
                return forceDone || (isLoaded && m_DependenciesIsDone);
            }
        }

        public bool isLoaded
        {
            get
            {
                return m_State == State.Loaded || m_State == State.Completed;
            }
        }

        public bool isDependenciesComplete
        {
            get
            {
                if (m_DependenciesIsLoaded)
                {
                    if (m_DependencyLoaders != null)
                    {
                        foreach (AssetBundleAsyncLoader loader in m_DependencyLoaders)
                        {
                            if (!loader.isDependenciesComplete)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
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
                    else
                    {
                        m_DependenciesIsDone = true;
                        m_DependenciesIsLoaded = true;
                    }
                    //AssetBundle不需要等待，依赖资源加载完成。
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

        public override void Update()
        {
            switch (m_State)
            {
                case State.Loaded:
                    Debug.LogFormat("{0},{1},{2}", info.fullName, m_WaitDependencyLoadCount, isDone);
                    if (isDone)
                    {
                        //Complete();
                    }
                    break;
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
            m_WaitDependencyLoadCount = dependencies.Length;
            m_WaitDependencyCompleteCount = dependencies.Length;

            Debug.Log("Load Dependencies " + dependencies.Length + "," + Time.frameCount);

            m_DependencyLoaders = ListPool<AssetBundleAsyncLoader>.Get();
            m_DependenciesIsLoaded = false;
            m_DependenciesIsDone = false;

            for (int i = 0, l = dependencies.Length; i < l; ++i)
            {
                string dep = dependencies[i];

                //if (dep.Contains("blue_s"))
                //{
                //    assetManager.StartCoroutine(testLoader(dep));
                //    continue;
                //}

                AssetBundleAsyncLoader depLoader = assetManager.LoadAssetBundle(dep, false, OnDependencyComplete) as AssetBundleAsyncLoader;
                if (depLoader != null)
                {
                    depLoader.autoRelease = false;
                    depLoader.onAssetBundleLoaded += OnDependencyLoaded;
                }
                m_DependencyLoaders.Add(depLoader);
            }
        }

        IEnumerator testLoader(string dep)
        {
            yield return new WaitForSeconds(0.5f);

            AssetBundleAsyncLoader depLoader = assetManager.LoadAssetBundle(dep, false, OnDependencyComplete) as AssetBundleAsyncLoader;
            if (depLoader != null)
            {
                depLoader.autoRelease = false;
                depLoader.onAssetBundleLoaded += OnDependencyLoaded;
            }
            m_DependencyLoaders.Add(depLoader);
        }

        protected void OnDependencyComplete(AssetBundleReference abr)
        {
            m_Dependencies.Add(abr);

            Debug.Log("DependencyComplete " + abr.name + "," + Time.frameCount);

            if (--m_WaitDependencyCompleteCount == 0)
            {
                m_DependenciesIsDone = true;
                if (m_Result != null)
                {
                    m_Result.AddDependencies(m_Dependencies);
                    Complete();
                }
            }
        }

        protected void OnDependencyLoaded(AssetBundleAsyncLoader loader)
        {
            Debug.Log("DependencyLoaded " + loader.info.fullName + "," + Time.frameCount);
            if (--m_WaitDependencyLoadCount == 0)
            {
                m_DependenciesIsLoaded = true;
            }
        }

        protected void OnBundleRequestComplete(Request request)
        {
            Debug.Log("BundleRequestComplete " + info.fullName + "," + Time.frameCount);
            if (!request.haveError)
            {
                state = State.Loaded;
                //Create result
                m_Result = new AssetBundleReference(request.assetBundle, info != null ? info.fullName : "");
                m_Result.AddTags(paramTags);
                if (m_DependenciesIsDone)
                {
                    m_Result.AddDependencies(m_Dependencies);
                    Complete();
                }
            }
            else
            {
                Debug.LogError("AssetBundleLoader fail load " + info.fullName);
                Error();
            }
        }

        public override void Complete()
        {
            state = State.Completed;
            DoLoadComplete();
        }

        public override void Error()
        {
            state = State.Error;
            DoLoadComplete();
        }

        Request LoadFromFile(string path)
        {
            Request request = RequestManager.CreateBundleCreateRequest(path);
            request.onComplete += OnBundleRequestComplete;
            assetManager.requestManager.ActiveRequest(request);
            return request;
        }

        Request LoadFromPackage(string path)
        {
            Request request = RequestManager.CreateBundleWebRequest(path);
            request.onComplete += OnBundleRequestComplete;
            assetManager.requestManager.ActiveRequest(request);
            return request;
        }

        public override void Clean()
        {
            onAssetBundleLoaded = null;
            m_WaitDependencyLoadCount = 0;
            m_DependenciesIsLoaded = false;
            m_WaitDependencyCompleteCount = 0;
            m_DependenciesIsDone = false;
            m_DependencyLoaders = null;

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
                m_Result = value;
            }
        }
    }
}