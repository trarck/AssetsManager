using System;
using System.Collections;
using System.Collections.Generic;

namespace YH.AssetManager
{
    public abstract class Loader : DataEnumerator
    {
        //public Action<AssetBundleReference> onComplete;

        //public int assetlevel { get; set; }
        //public string assetTag { get; set; }
        //public virtual AssetBundleInfo info { get; set; }

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
            //assetlevel = 0;
            //assetTag = null;
        }

        public virtual AssetBundleReference GetResult()
        {
            return null;
        }
    }
}
