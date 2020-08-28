using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YH.AssetManage
{
    public interface IRequestManager
    {
        void Init();
        void Update(float deltaTime);
        void Clean();
        Request CreateAssetBundleRequest(AssetBundleInfo assetBundleInfo);

		Request CreateAssetRequest(AssetBundle assetBundle, string assetName, Type type);
		Request CreateAssetRequest(string resourcePath, Type type);
		Request CreateAssetRequest();

		void ActiveRequest(Request request);

        void OnApplicationPause(bool pause);
        void OnApplicationQuit();
    }
}
