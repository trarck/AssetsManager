using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public class AssetLoader : Loader
    {
        public Action<UnityEngine.Object> onComplete;

        public AssetInfo info { get; set; }

        public AssetBundle assetBundle { get; set; }

        public override bool isDone
        {
            get
            {
                return false;
            }
        }

        public override void Start()
        {
            if (assetBundle != null)
            {

            }
        }

        public override void Complete()
        {

        }

        public override void Error()
        {

        }

        public override void Clean()
        {

        }

        public override AssetBundleReference GetResult()
        {
            return null;
        }
    }
}
