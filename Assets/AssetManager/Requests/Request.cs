using System;
using System.Collections;
using UnityEngine;

namespace YH.AssetManage
{
    public abstract class Request : IEnumerator
    {
        public event Action<Request> onComplete;
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

        public virtual void Abort()
        {

        }

        public virtual void Update()
        {
            AMDebug.LogFormat("[AssetManage]{0}", ToString());
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
