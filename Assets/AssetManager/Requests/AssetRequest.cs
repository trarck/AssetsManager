using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{

    public class AssetLoaderRequest : Request
    {
        AssetBundleRequest m_Request;

        public string assetName { get; set; }
        public Type type { get; set; }

        public override bool isDone
        {
            get
            {
                if (m_Request != null)
                {
                    return m_Request.isDone;
                }
                else
                {
                    return false;
                }
            }
        }

        public override float progress
        {
            get
            {
                if (m_Request != null)
                {
                    return m_Request.progress;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override UnityEngine.Object data
        {
            get
            {
                return m_Request != null ? m_Request.asset : null;
            }
        }

        public AssetLoaderRequest()
        {

        }

        public AssetLoaderRequest(AssetBundle assetBundle, string assetName,Type type)
        {
            this.assetBundle = assetBundle;
            this.assetName = assetName;
            this.type = type;
        }

        public override void Start()
        {
            if (type == null)
            {
                m_Request = assetBundle.LoadAssetAsync(assetName);
            }
            else
            {
                m_Request = assetBundle.LoadAssetAsync(assetName, type);
            }
        }

        public override bool haveError
        {
            get
            {
                if (m_Request != null && isDone)
                {
                    return m_Request.asset == null;
                }
                return false;
            }
        }

        public override void Clean()
        {
            m_Request = null;
            assetName = null;
            type = null;
            base.Clean();
        }
        public override string ToString()
        {
            return string.Format("AssetLoaderRequest:{0},isDone:{1}", assetName, isDone);
        }
    }

    public class ResouceLoaderRequest : Request
    {
        ResourceRequest m_Request;

        public string resourcePath { get; set; }
        public Type type { get; set; }

        public override bool isDone
        {
            get
            {
                if (m_Request != null)
                {
                    return m_Request.isDone;
                }
                else
                {
                    return false;
                }
            }
        }

        public override float progress
        {
            get
            {
                if (m_Request != null)
                {
                    return m_Request.progress;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override UnityEngine.Object data
        {
            get
            {
                return m_Request != null ? m_Request.asset : null;
            }
        }

        public ResouceLoaderRequest()
        {

        }

        public ResouceLoaderRequest(string resourcePath, Type type)
        {
            this.resourcePath = resourcePath;
            this.type = type;
        }

        public override void Start()
        {
            if (type == null)
            {
                m_Request = Resources.LoadAsync(resourcePath);
            }
            else
            {
                m_Request = Resources.LoadAsync(resourcePath, type);
            }
        }


        public override bool haveError
        {
            get
            {
                if (m_Request != null && isDone)
                {
                    return m_Request.asset == null;
                }
                return false;
            }
        }

        public override void Clean()
        {
            m_Request = null;
            resourcePath = null;
            type = null;
            base.Clean();
        }

        public override string ToString()
        {
            return string.Format("ResouceLoaderRequest:{0},isDone:{1}",  resourcePath, isDone);
        }
    }
}
