using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    public abstract class LoaderRequest : DataEnumerator
    {
       
    }

    public class WWWRequest : LoaderRequest
    {
        WWW m_Www;

        public override bool isDone
        {
            get
            {
                if(m_Www!=null)
                {
                    return m_Www.isDone;
                }
                else
                {
                    return false;
                }
                
            }
        }

        public override AssetBundle assetBundle
        {
            get
            {
                return m_Www!=null? m_Www.assetBundle:null;
            }
        }

        public WWWRequest(WWW www)
        {
            m_Www = www;
        }
    }

    public class BundleLoaderRequest : LoaderRequest
    {
        AssetBundleCreateRequest m_CreateRequest;

        public override bool isDone
        {
            get
            {
                if (m_CreateRequest != null)
                {
                    return m_CreateRequest.isDone;
                }
                else
                {
                    return false;
                }
            }
        }

        public override AssetBundle assetBundle
        {
            get
            {
                return m_CreateRequest != null ? m_CreateRequest.assetBundle : null;
            }
        }

        public BundleLoaderRequest(AssetBundleCreateRequest createRequest)
        {
            m_CreateRequest = createRequest;
        }
    }

    public class AssetLoaderRequest : LoaderRequest
    {
        AssetBundleRequest m_Request;

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

        public override UnityEngine.Object data
        {
            get
            {
                return m_Request != null ? m_Request.asset : null;
            }
        }

        public AssetLoaderRequest(AssetBundleRequest request)
        {
            m_Request = request;
        }
    }

    public class ResouceLoaderRequest : LoaderRequest
    {
        ResourceRequest m_Request;

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

        public override UnityEngine.Object data
        {
            get
            {
                return m_Request != null ? m_Request.asset : null;
            }
        }

        public ResouceLoaderRequest(ResourceRequest request)
        {
            m_Request = request;
        }
    }

    public class SyncLoaderRequest : LoaderRequest
    {
        public override bool isDone
        {
            get
            {
                return true;
            }
        }
    }

    public class SceneLoaderRequest : LoaderRequest
    {
        public override bool isDone
        {
            get
            {
                return true;
            }
        }
    }
}
