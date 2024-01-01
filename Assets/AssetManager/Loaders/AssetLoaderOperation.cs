using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YH.AssetManage
{
    public struct AssetLoaderOperation
    {
        public AssetLoader loader;

        public static AssetLoaderOperation Empty = new AssetLoaderOperation(null);

        public AssetLoaderOperation(AssetLoader loader)
        {
            this.loader = loader;
        }

        public bool IsValid()
        {
            return loader != null;
        }


        public void Stop()
        {

        }
    }
}
