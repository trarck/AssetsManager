using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.AssetManager
{
    internal static class LoaderPool
    {
        // Object pool to avoid allocations.
        public static readonly ObjectPool<AssetBundleAsyncLoader> AssetBundleAsyncLoader = new ObjectPool<AssetBundleAsyncLoader>(null, l=>l.Clean());
        public static readonly ObjectPool<AssetAsyncLoader> AssetAsyncLoader = new ObjectPool<AssetAsyncLoader>(null, l => l.Clean());
    }
}
