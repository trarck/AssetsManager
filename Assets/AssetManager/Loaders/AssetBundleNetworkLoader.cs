using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    /// <summary>
    /// 从网络上加载AssetBundle
    /// </summary>
    public class AssetBundleNetworkLoader : AssetBundleAsyncLoader
    {
        protected override void LoadBundle()
        {
            string url = AssetPaths.GetUrl(info.fullName);
            string savePath = AssetPaths.ToBundlePath(info.fullName);
#if ASSETMANAGER_LOG
            Debug.Log("LoadBundle " + url + ",save to " + savePath+"," + Time.frameCount);
#endif
            LoadAndSave(url, savePath);
        }

        protected Request LoadAndSave(string url,string path)
        {
            Request request = RequestManager.CreateBundleWebSaveRequest(url,path, info != null ? info.hash : null);
            request.onComplete += OnBundleRequestComplete;
            assetManager.requestManager.ActiveRequest(request);
            return request;
        }
    }
}