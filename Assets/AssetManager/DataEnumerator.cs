using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public abstract class DataEnumerator : IEnumerator
    {
        public object Current
        {
            get
            {
                return null;
            }
        }

        public abstract bool isDone { get; }
        public abstract bool haveError { get; }

        public virtual Object data { get; set; }
        public virtual AssetBundle assetBundle { get; set; }

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
            
        }
    }
}
