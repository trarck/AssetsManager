#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YH.AssetManage
{
    public class AssetBundleEmptyLoader : AssetBundleAsyncLoader
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
                Debug.LogErrorFormat("[AssetManage]Load AssetBundle mode not open {0}", info.fullName);
            }
            Complete();
        }

    }
}
#endif