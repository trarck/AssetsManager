using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManage
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
        //如果有依赖项加载失败根据AMSetting.BreakOnBundleLoadDependencyError来判断当前AssetBundle是否加载成功。
        //通常要判断失败，除非有其他方法来加载失败的依赖项。
        bool m_DependenciesHaveError = false;

        //由于整个loader使用了缓存，这里可以不使用缓存。
        List<AssetBundleAsyncLoader> m_DependencyLoaders = null;

        Request m_Request = null;
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
                    Debug.LogError("[AssetManage]Load AssetBundle with no info");
                    Error();
                }
            }
            else if (isFinishedState)
            {
                DoLoadComplete();
            }
        }

        public override void Abort()
        {
            //如果已经完成或出错，则无效。
            if (m_State == State.Error || m_State == State.Completed)
            {
                return;
            }

            base.Abort();

            //清除未完成的loader
            if (m_DependencyLoaders!=null && m_DependencyLoaders.Count > 0)
            {
                for (int i = 0; i < m_DependencyLoaders.Count; ++i)
                {
                    AssetBundleAsyncLoader depLoader = m_DependencyLoaders[i];
                    depLoader.onAssetBundleLoaded -= OnDependencyLoaded;
                    depLoader.onBeforeComplete -= OnBeforeDependencyComplete;
                    depLoader.onComplete -= OnDependencyComplete;
                    depLoader.DecreaseLoadingCount();
                }
                m_DependencyLoaders.Clear();
            }

            //清除已经加载的AssetBundle
            ClearDependencies();

            //清除本身资源
            if (m_Request != null)
            {
                m_Request.onComplete -= OnBundleRequestComplete;
                m_Request = null;
            }

            //执行一次加载完成
            DoLoadComplete();
        }

        protected virtual void LoadBundle()
        {
            if (loaderManager!=null && loaderManager.requestManager!=null)
            {
                m_Request = loaderManager.requestManager.CreateAssetBundleRequest(info);
                m_Request.onComplete += OnBundleRequestComplete;
				loaderManager.requestManager.ActiveRequest(m_Request);
            }
            else
            {
#if ASSETMANAGER_LOG_ON
            Debug.LogError("[AssetManage]LoadBundle: no request manager.");
#endif
            }
        }

        protected virtual void LoadDependencies()
        {
            string[] dependencies = info.dependencies;
            m_WaitDependencyLoadCount = dependencies.Length;
            m_WaitDependencyCompleteCount = dependencies.Length;
            m_DependenciesHaveError = false;

#if ASSETMANAGER_LOG_ON
            Debug.LogFormat("[AssetManage]Load Dependencies {0}---{1}" , dependencies.Length , Time.frameCount);
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

				AssetBundleAsyncLoader depLoader = loaderManager.LoadAssetBundleAsync(dep, 0, AMSetting.CacheDependencyBundle, OnDependencyComplete) as AssetBundleAsyncLoader;
                if (depLoader != null)
                {
                    depLoader.onAssetBundleLoaded += OnDependencyLoaded;
                    depLoader.onBeforeComplete += OnBeforeDependencyComplete;
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

        protected void OnDependencyLoaded(AssetBundleAsyncLoader loader)
        {
#if ASSETMANAGER_LOG_ON
            Debug.LogFormat("[AssetManage]DependencyLoaded {0}=>{1}---{2}" , info!=null?info.fullName:"Null" , loader.info != null ? loader.info.fullName : "Null", Time.frameCount);
#endif
            if (--m_WaitDependencyLoadCount == 0)
            {
                m_DependenciesIsLoaded = true;
            }
        }

        protected void OnBeforeDependencyComplete(AssetBundleLoader loader)
        {
#if ASSETMANAGER_LOG_ON
            Debug.LogFormat("[AssetManage]OnBeforeDependencyComplete remove from loader {0}=>{1}---{2} ", info != null ? info.fullName : "Null", loader.info != null ? loader.info.fullName : "Null", Time.frameCount);
#endif

            //依赖项已经加载完成要从m_DependencyLoaders里移除。
            for (int i = 0, l = m_DependencyLoaders.Count; i < l; ++i)
            {
                if (m_DependencyLoaders[i] == loader)
                {
                    m_DependencyLoaders.RemoveAt(i);
                    break;
                }
            }
        }

        protected void OnDependencyComplete(AssetBundleReference abr)
        {
            if (abr != null && !abr.IsEmpty())
            {
                AddDependency(abr);
#if ASSETMANAGER_LOG_ON
                Debug.LogFormat("[AssetManage]DependencyComplete {0}=>{1},{2}---{3} " , info.fullName , abr.name , m_WaitDependencyCompleteCount, Time.frameCount);
#endif
            }
            else
            {
                Debug.LogError("[AssetManage]Download dependency error");
                m_DependenciesHaveError = true;
            }

            if (--m_WaitDependencyCompleteCount == 0)
            {
                m_DependenciesIsDone = true;
                if (m_State == State.Loaded && m_Result != null)
                {
                    DoAllComplete();
                }
            }
        }

        protected void OnBundleRequestComplete(Request request)
        {
            if (m_Request != null)
            {
                m_Request.onComplete -= OnBundleRequestComplete;
                m_Request = null;
            }

#if ASSETMANAGER_LOG_ON
            Debug.LogFormat("[AssetManage]BundleRequestComplete {0}---{1}" , info.fullName ,Time.frameCount);
#endif
            if (!request.haveError)
            {
                state = State.Loaded;
                if (onAssetBundleLoaded != null)
                {
                    onAssetBundleLoaded(this);
                }
                
                //Create result
                result = new AssetBundleReference(request.assetBundle, info != null ? info.fullName : "");
                m_Result.AddTags(paramTags);
                if (m_DependenciesIsDone )//|| (m_DependenciesIsLoaded && DeepCheckDependenciesComplete()))
                {
                    DoAllComplete();
                }
            }
            else
            {
                Debug.LogErrorFormat("[AssetManage]BundleRequest fail {0}---{1}" , info.fullName,Time.frameCount);
                Error();
            }
        }

        protected void DoAllComplete()
        {
            m_Result.AddDependencies(m_Dependencies);
            ClearDependencies();
            ClearDependencyLoaders();
            if(m_DependenciesHaveError && AMSetting.BreakOnBundleLoadDependencyError)
            {
                Error();
            }
            else
            {
                Complete();
            }
        }

        //protected Request LoadFromFile(string path)
        //{
        //    m_Request = RequestManager.CreateBundleCreateRequest(path);
        //    m_Request.onComplete += OnBundleRequestComplete;
        //    assetManager.requestManager.ActiveRequest(m_Request);
        //    return m_Request;
        //}

        //protected Request LoadFromWeb(string path)
        //{
        //    m_Request = RequestManager.CreateBundleWebRequest(path, info!=null?info.hash:null);
        //    m_Request.onComplete += OnBundleRequestComplete;
        //    assetManager.requestManager.ActiveRequest(m_Request);
        //    return m_Request;
        //}

        protected void ClearDependencyLoaders()
        {
            if (m_DependencyLoaders != null && m_DependencyLoaders.Count>0)
            {
                //foreach(var loader in m_DependencyLoaders)
                //{
                //    loader.onAssetBundleLoaded -= OnDependencyLoaded;
                //    loader.onBeforeComplete -= OnBeforeDependencyComplete;
                //    loader.onComplete -= OnDependencyComplete;
                //}

                m_DependencyLoaders.Clear();
            }
        }

        public override void Clean()
        {
            onAssetBundleLoaded = null;
            m_WaitDependencyLoadCount = 0;
            m_DependenciesIsLoaded = false;
            m_WaitDependencyCompleteCount = 0;
            m_DependenciesIsDone = false;
            m_DependenciesHaveError = false;
            ClearDependencyLoaders();

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
                //m_Result = value;
                base.result = value;
            }
        }
    }
}