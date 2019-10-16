using System;
using System.Collections;
using UnityEngine;

namespace YH.AssetManager
{
    public abstract class Request : IEnumerator
    {
        public Action<Request> onComplete;
        public bool autoRelease = true;

        #region Enumertor
        public object Current
        {
            get
            {
                return null;
            }
        }

        public abstract bool isDone { get; }
        public abstract float progress { get; }
        public abstract bool haveError { get; }

        public virtual UnityEngine.Object data { get; set; }
        public virtual AssetBundle assetBundle { get; set; }

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {

        }
        #endregion

        public virtual void Start()
        {

        }

        public virtual void Update()
        {
            #if ASSETMANAGER_LOG
            Debug.LogFormat("{0},{1}", ToString(), Time.frameCount);
            #endif
        }

        public virtual void Complete()
        {
            if (onComplete!=null)
            {
                onComplete(this);
            }
        }

        public virtual void Clean()
        {
            onComplete=null;
            data = null;
            assetBundle = null;
            autoRelease = true;
        }
    }
}
