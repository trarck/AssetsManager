using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.AssetManager
{
    internal static class LoaderPool
    {
        // Object pool to avoid allocations.
        public static readonly ObjectPool<AssetBundleLoader> assetBundleLoader = new ObjectPool<AssetBundleLoader>(null, l=>l.Clean());
    }
}
