using System;
using System.Collections;
using System.Collections.Generic;

namespace YH.AssetManager
{
    public class AssetLoader : Loader
    {
        public Action<UnityEngine.Object> onComplete;

        public AssetInfo info { get; set; }

        public override bool isDone
        {
            get
            {
                return false;
            }
        }

        public override void Start()
        {
            if (string.IsNullOrEmpty(info.bundleName))
            {
                //load asset
            }
            else
            {
                //load bundle
            }

            if (m_AssetPath.Contains("://"))
            {
                this.LoadFromPackage(m_AssetPath);
            }
            else
            {
                this.LoadFromFile(m_AssetPath);
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
