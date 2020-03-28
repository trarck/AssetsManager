using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YH.AssetManage
{
    public interface IRequestManager
    {
        void Update(float deltaTime);
        void Clean();
        Request CreateAssetBundleRequest(AssetBundleInfo assetBundleInfo);
        void ActiveRequest(Request request);
    }
}
