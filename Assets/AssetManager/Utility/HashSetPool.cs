using System;
using System.Collections.Generic;
using UnityEngine;

namespace YH.AssetManager
{
    internal static class HashSetPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<HashSet<T>> s_HashSetPool = new ObjectPool<HashSet<T>>(null, l => l.Clear());

        public static HashSet<T> Get()
        {
            return s_HashSetPool.Get();
        }

        public static void Release(HashSet<T> toRelease)
        {
            s_HashSetPool.Release(toRelease);
        }
    }
}
