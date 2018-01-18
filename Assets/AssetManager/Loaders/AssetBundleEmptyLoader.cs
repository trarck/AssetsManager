#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YH.AssetManager
{
    public class AssetBundleEmptyLoader : AssetBundleLoader
    {
        public override bool isDone
        {
            get
            {
                return true;
            }
        }

        public override void Start()
        {
            if (info != null)
            {
                Debug.LogErrorFormat("Load AssetBundle mode not open {0}", info.fullName);
            }
            Error();
        }

        public override void Error()
        {
            state = State.Error;
            DoLoadComplete();
        }
    }
}
#endif