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
        public override void Start()
        {
            m_LoaderRequest = new EmptyLoaderRequest();
        }
    }
}
#endif