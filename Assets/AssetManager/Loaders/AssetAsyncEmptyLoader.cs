#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YH.AssetManage
{
    public class AssetAsyncEmptyLoader : AssetAsyncLoader
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
            Complete();
        }

    }
}
#endif