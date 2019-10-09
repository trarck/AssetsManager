using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    /// <summary>
    /// 异步加载AssetBundle
    /// AssetBundle和依赖项同时加载，判断一个AssetBundle是加载完成，要检查自己是否加载和所有依赖是否完成。
    /// 不要有循环依赖的AssetBundle
    /// </summary>
    public class AssetBundleAsyncLoader : AssetBundleLoader
    {
        public event Action<AssetBundleAsyncLoader> onAssetBundleLoaded;

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
                return forceDone || m_State == State.Completed || m_State == State.Error;//(isLoaded && m_DependenciesIsDone);
            }
        }

        public bool isComplete
        {
            get
            {
                return m_State == State.Completed || m_State == State.Error;
            }
        }

        public bool isLoaded
        {
            get
            {
                return m_State == State.Loaded || m_State == State.Completed || m_State == State.Error;
            }
        }

        /// <summary>
        /// 深度检查所有的子依赖都在完成状态，虽然可以检查带循环引用的，但是不推荐有循环引用，可以在生成AssetBundle的时候进行检查。
        /// 循环引用不仅引响加载，还引响回收。
        /// </summary>
        /// <param name="checkeds"></param>
        /// <returns></returns>
        public bool DeepCheckDependenciesComplete(HashSet<AssetBundleAsyncLoader> checkeds=null)
        {
            if (m_DependenciesIsLoaded)
            {
                if (checkeds == null)
                {
                    checkeds = new HashSet<AssetBundleAsyncLoader>();
                }

                if (checkeds.Contains(this))
                {
                    return true;
                }

                checkeds.Add(this);

                if (m_DependencyLoaders != null)
                {
                    foreach (AssetBundleAsyncLoader loader in m_DependencyLoaders)
                    {
    
                        if (!loader.isComplete && !loader.DeepCheckDependenciesComplete(checkeds))
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

        protected virtual void LoadBundle()
        {
            string assetPath = AssetPaths.GetFullPath(info.fullName);
#if ASSETMANAGER_LOG
            Debug.Log("LoadBundle " + assetPath + "," + Time.frameCount);
#endif
            if (assetPath.Contains("://"))
            {
                LoadFromWeb(assetPath);
            }
            else
            {
                LoadFromFile(assetPath);
            }
        }

        protected virtual void LoadDependencies()
        {
            string[] dependencies = info.dependencies;
            m_WaitDependencyLoadCount = dependencies.Length;
            m_WaitDependencyCompleteCount = dependencies.Length;
#if ASSETMANAGER_LOG
            Debug.Log("Load Dependencies " + dependencies.Length + "," + Time.frameCount);
#endif
            if (m_DependencyLoaders == null)
            {
                m_DependencyLoaders = new List<AssetBundleAsyncLoader>();
            }
            else
            {
                m_DependencyLoaders.Clear();
            }

            ResetDependencies();

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

        //IEnumerator testLoader(string dep)
        //{
        //    yield return new WaitForSeconds(0.5f);

        //    AssetBundleAsyncLoader depLoader = assetManager.LoadAssetBundle(dep, false, OnDependencyComplete) as AssetBundleAsyncLoader;
        //    if (depLoader != null)
        //    {
        //        depLoader.autoRelease = false;
        //        depLoader.onAssetBundleLoaded += OnDependencyLoaded;
        //    }
        //    m_DependencyLoaders.Add(depLoader);
        //}

        protected void OnDependencyComplete(AssetBundleReference abr)
        {
            AddDependency(abr);
#if ASSETMANAGER_LOG
            Debug.Log("DependencyComplete "+info.fullName+ "=>" + abr.name + "," + Time.frameCount+",("+ m_WaitDependencyCompleteCount+")");
#endif
            if (--m_WaitDependencyCompleteCount == 0)
            {
                m_DependenciesIsDone = true;
                if (m_State == State.Loaded && m_Result != null)
                {
                    m_Result.AddDependencies(m_Dependencies);
                    ClearDependencies();
                    Complete();
                }
            }
        }

        protected void OnDependencyLoaded(AssetBundleAsyncLoader loader)
        {
#if ASSETMANAGER_LOG
            Debug.Log("DependencyLoaded " + info.fullName + "->" + loader.info.fullName + "," + Time.frameCount);
#endif
            if (--m_WaitDependencyLoadCount == 0)
            {
                m_DependenciesIsLoaded = true;
            }
        }

        protected void OnBundleRequestComplete(Request request)
        {
#if ASSETMANAGER_LOG
            Debug.Log("BundleRequestComplete " + info.fullName + "," + Time.frameCount);
#endif
            if (!request.haveError)
            {
                state = State.Loaded;
                if (onAssetBundleLoaded != null)
                {
                    onAssetBundleLoaded(this);
                }
                
                //Create result
                m_Result = new AssetBundleReference(request.assetBundle, info != null ? info.fullName : "");
                m_Result.AddTags(paramTags);
                if (m_DependenciesIsDone )//|| (m_DependenciesIsLoaded && DeepCheckDependenciesComplete()))
                {
                    m_Result.AddDependencies(m_Dependencies);
                    ClearDependencies();
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

        protected Request LoadFromFile(string path)
        {
            Request request = RequestManager.CreateBundleCreateRequest(path);
            request.onComplete += OnBundleRequestComplete;
            assetManager.requestManager.ActiveRequest(request);
            return request;
        }

        protected Request LoadFromWeb(string path)
        {
            Request request = RequestManager.CreateBundleWebRequest(path, info!=null?info.hash:null);
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
            if (m_DependencyLoaders != null)
            {
                m_DependencyLoaders.Clear();
            }

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