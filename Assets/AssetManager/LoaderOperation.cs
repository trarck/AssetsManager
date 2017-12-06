using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class LoaderOperation : IEnumerator
    {
        ILoader m_Loader;

        public object Current
        {
            get
            {
                return null;
            }
        }

        public LoaderOperation(ILoader loader)
        {
            m_Loader = loader;
        }

        public bool MoveNext()
        {
            return m_Loader != null || !m_Loader.isDone;
        }

        public void Reset()
        {
            
        }
    }
}
