using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YH.AssetManager
{
    internal static class LoaderPool
    {
        // Object pool to avoid allocations.
        public static readonly ObjectPool<AsyncLoader> asyncLoaderPool = new ObjectPool<AsyncLoader>(null, l=>l.Clean());
    }
}
