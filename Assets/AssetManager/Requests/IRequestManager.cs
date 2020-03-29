using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YH.AssetManage
{
    public interface IRequestManager
    {
        void Init();
        void Update(float deltaTime);
        void Clean();
        Request CreateAssetBundleRequest(AssetBundleInfo assetBundleInfo);
        void ActiveRequest(Request request);

        void OnApplicationPause(bool pause);
        void OnApplicationQuit();
    }
}
