using System;
using System.Collections;
using System.Collections.Generic;

namespace YH.AssetManager
{
    public abstract class Loader : DataEnumerator
    {
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

        }

        public virtual AssetBundleReference GetResult()
        {
            return null;
        }
    }
}
