using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YH.AssetManager
{
    public class AssetReference:BaseReference
    {

        public AssetBundleReference assetBundleReference { get; set; }

        public string assetPath { get; set; }

        public Object asset { get; set; }

        public AssetReference(Object asset, string assetPath)
        {
            this.asset = asset;
            this.assetPath = assetPath;
        }
        
        public override void Retain()
        {
            base.Retain();

            if (assetBundleReference != null)
            {
                assetBundleReference.Retain();
            }
        }

        public override void Release()
        {
            if (assetBundleReference != null)
            {
                assetBundleReference.Release();
            }
            base.Release();
        }

        public override void Retain(Object owner)
        {
            base.Retain(owner);

            if (assetBundleReference != null)
            {
                assetBundleReference.Retain(owner);
            }
        }

        public override void Release(Object owner)
        {
            if (assetBundleReference != null)
            {
                assetBundleReference.Release(owner);
            }
            base.Release(owner);
        }

        
        public override void Dispose()
        {
            if (asset == null)
            {
                Resources.UnloadAsset(asset);
                asset = null;
            }
            asset = null;
            assetBundleReference = null;
            assetPath = null;

            base.Dispose();
        }


        public override void Reset()
        {
            base.Reset();
            asset = null;
            assetBundleReference = null;
            assetPath = null;
        }      
    }
}
