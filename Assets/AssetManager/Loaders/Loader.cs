using System;
using System.Collections;
using System.Collections.Generic;

namespace YH.AssetManager
{
    public abstract class Loader : DataEnumerator
    {
        //public Action<AssetBundleReference> onComplete;

        public int paramLevel { get; set; }
        public string paramTag { get; set; }

        public bool forceDone { get; set; }

        public virtual void Start()
        {

        }

        public virtual void Complete()
        {

        }

        public virtual void Error()
        {

        }

        public virtual void Clean()
        {
            paramLevel = 0;
            paramTag = null;
        }
        //public virtual AssetBundleReference GetResult()
        //{
        //    return null;
        //}
    }
}
